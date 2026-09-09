/*
    IntroPrimaryScript.cs
    - Only runs after player is unlocked in IntroSequence
    - Handles sitting down/up AND control interactions
    - Manages the HUD display for control interaction
    - Sends user inputs to control script if looking at said control and within RAYCAST_RANGE
    - Handles transmitting IK targets for hand movement animations
    Contributor(s): Jake Schott, John Aylward
    Last Updated: 9/6/2026
*/

using UnityEngine;

public class IntroPrimaryScript : PrimaryScript
{
    public override void onShiftChange()
    {
        bool can_shift_left = false;
        bool can_shift_right = false;
        GetComponent<SecondaryScript>().updateShiftIndicators(false, (IntroSequenceManager.SEAT_COORDINATES[curr_seat].Length > 0), can_shift_left, can_shift_right);
    }

    public override bool isCaptainMode()
    {
        return false;
    }

    //returns seat index of closest seat or -1 if none
    protected override int getClosestSeat()
    {
        return ReferenceAssistor.Instance.intro_sequence_manager.checkSeats(player_prefab.transform.position);
    }

    //returns corresponding color of seat
    protected override Color getSeatColor(int seat)
    {
        return IntroSequenceManager.SEAT_COLORS[seat];
    }

    //returns corresponding name of seat
    protected override string getSeatName(int seat)
    {
        return IntroSequenceManager.SEAT_NAMES[seat];
    }

    //returns true if seat claim was successful
    protected override bool claimSeat(int seat)
    {
        return true;
    }

    //starts sit down animation
    protected override void startSitDownAnimation()
    {
        //freeze player
        PlayerManager.freezePlayer(player_prefab);

        //update top right indicator and borders
        GetComponent<SecondaryScript>().onStationChange(getSeatColor(curr_seat), null);

        //trigger get up animation and positional adjustments (player, seat)
        bool is_left = ReferenceAssistor.Instance.intro_sequence_manager.getSitDownDirection(curr_seat, player_prefab.transform.position);
        GameObject physical_seat = ReferenceAssistor.Instance.intro_sequence_manager.physical_seats[curr_seat];
        GameObject animation_start_point = ReferenceAssistor.Instance.intro_sequence_manager.getSitDownPosition(curr_seat, player_prefab.transform.position);
        player_prefab.GetComponent<PlayerMove>().TriggerSitDownAnimation(is_left, false, physical_seat, animation_start_point);
    }

    //handles script-specific things on completion of sit down animation
    protected override void handleCompletedSitDown()
    {
        //set captain mode
        player_prefab.GetComponent<CameraMove>().SetCaptainMode(false);

        //push the seat in to complete the sit down process
        GameObject physical_seat = ReferenceAssistor.Instance.intro_sequence_manager.physical_seats[curr_seat];
        Vector2 push_adjustment = IntroSequenceManager.SEAT_PUSH_IN_ADJUSTMENTS[curr_seat];
        player_prefab.GetComponent<PlayerMove>().SeatPush(physical_seat, push_adjustment);
    }

    //starts get up animation
    protected override void startGetUpAnimation()
    {
        //trigger get up animation and positional adjustments (player, seat)
        bool is_left = ReferenceAssistor.Instance.intro_sequence_manager.getGetUpDirection(curr_seat);
        GameObject physical_seat = ReferenceAssistor.Instance.intro_sequence_manager.physical_seats[curr_seat];
        Vector2 push_adjustment = IntroSequenceManager.SEAT_PUSH_IN_ADJUSTMENTS[curr_seat];
        player_prefab.GetComponent<PlayerMove>().TriggerGetUpAnimation(is_left, false, physical_seat, push_adjustment);
    }

    //handles script-specific things on completion of get up animation
    protected override void handleCompletedGetUp()
    {
        //unfreeze player
        PlayerManager.unfreezePlayer(player_prefab);

        //update camera
        player_prefab.GetComponent<CameraMove>().parentRotationLock = false;
        float[] rotations = new float[] { 90.0f };
        player_prefab.GetComponent<CameraMove>().UnlockCamera(new Vector2(rotations[curr_seat], 30.0f));
    }

    //called when hitting shift while sitting down
    protected override void attemptSeatShift()
    {

    }

    protected override HUDInfo checkRayTarget()
    {
        current_controllable = ReferenceAssistor.Instance.module_handlers[curr_seat].GetComponent(current_ray_target.transform.GetChild(0).name) as IControllable;
        current_describable = ReferenceAssistor.Instance.module_handlers[curr_seat].GetComponent(current_ray_target.transform.GetChild(0).name) as IDescribable;

        HUDInfo temp_info = null;
        if (current_controllable != null) //IControllable
        {
            temp_info = current_controllable.getHUDinfo(current_ray_target.gameObject);
        }
        else //IDescribable
        {
            temp_info = current_describable.getHUDinfo(current_ray_target.gameObject);
        }

        return temp_info;
    }
}