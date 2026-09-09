/*
    BridgePrimaryScript.cs
    - Only runs after scene is loaded in as BridgeEnvironment
    - Handles sitting down/up AND control interactions
    - Manages the HUD display for control interaction
    - Sends user inputs to control script if looking at said control and within RAYCAST_RANGE
    - Handles transmitting IK targets for hand movement animations
    Contributor(s): Jake Schott, John Aylward
    Last Updated: 9/6/2026
*/

using System.Collections;
using UnityEngine;

public class BridgePrimaryScript : PrimaryScript
{
    public override void onShiftChange()
    {
        bool can_shift_left = ReferenceAssistor.Instance.seat_manager.canShiftLeft(curr_seat);
        bool can_shift_right = ReferenceAssistor.Instance.seat_manager.canShiftRight(curr_seat);
        GetComponent<SecondaryScript>().updateShiftIndicators(player_prefab.GetComponent<PlayerMove>().IsShifting(), (curr_seat == 3), can_shift_left, can_shift_right);
    }

    public override bool isCaptainMode()
    {
        return (curr_seat == 3);
    }

    //returns seat index of closest seat or -1 if none
    protected override int getClosestSeat()
    {
        if (ReferenceAssistor.Instance.seat_manager == null)
        {
            return -1;
        }
        return ReferenceAssistor.Instance.seat_manager.checkSeats(player_prefab.transform.position);
    }

    //returns corresponding color of seat
    protected override Color getSeatColor(int seat)
    {
        return ReferenceAssistor.COLOR_OPTIONS[seat];
    }

    //returns corresponding name of seat
    protected override string getSeatName(int seat)
    {
        return ReferenceAssistor.STATION_NAMES[seat] + " STATION";
    }

    //returns true if seat claim was successful
    protected override bool claimSeat(int seat)
    {
        return ReferenceAssistor.Instance.seat_manager.claimSeatOccupancy(seat);
    }

    //starts sit down animation
    protected override void startSitDownAnimation()
    {
        //update top right indicator and borders
        GetComponent<SecondaryScript>().onStationChange(ReferenceAssistor.COLOR_OPTIONS[curr_seat], ReferenceAssistor.Instance.position_icons[curr_seat]);

        //trigger get up animation and positional adjustments (player, seat)
        bool is_left = ReferenceAssistor.Instance.seat_manager.getSitDownDirection(curr_seat, player_prefab.transform.position);
        GameObject physical_seat = ReferenceAssistor.Instance.seat_manager.physical_seats[curr_seat];
        GameObject animation_start_point = ReferenceAssistor.Instance.seat_manager.getSitDownPosition(curr_seat, player_prefab.transform.position);
        player_prefab.GetComponent<PlayerMove>().TriggerSitDownAnimation(is_left, (curr_seat == 3), physical_seat, animation_start_point);
    }

    //handles script-specific things on completion of sit down animation
    protected override void handleCompletedSitDown()
    {
        //if captain, trigger the seat enclosure animaiton
        if (curr_seat == 3)
        {
            ReferenceAssistor.Instance.seat_manager.encloseCaptainSeat();
        }

        //set captain mode
        player_prefab.GetComponent<CameraMove>().SetCaptainMode(curr_seat == 3);

        //push the seat in to complete the sit down process
        ReferenceAssistor.Instance.seat_manager.beginShift(curr_seat);
        GameObject physical_seat = ReferenceAssistor.Instance.seat_manager.physical_seats[curr_seat];
        Vector2 push_adjustment = SeatManager.SEAT_PUSH_IN_ADJUSTMENTS[curr_seat];
        player_prefab.GetComponent<PlayerMove>().SeatPush(physical_seat, push_adjustment);
    }

    //starts get up animation
    protected override void startGetUpAnimation()
    {
        //if captain, trigger the seat free animation
        if (curr_seat == 3)
        {
            ReferenceAssistor.Instance.seat_manager.releaseCaptainSeat();
        }

        //trigger get up animation and positional adjustments (player, seat)
        bool is_left = ReferenceAssistor.Instance.seat_manager.getGetUpDirection(curr_seat);
        GameObject physical_seat = ReferenceAssistor.Instance.seat_manager.physical_seats[curr_seat];
        Vector2 push_adjustment = SeatManager.SEAT_PUSH_IN_ADJUSTMENTS[curr_seat];
        player_prefab.GetComponent<PlayerMove>().TriggerGetUpAnimation(is_left, (curr_seat == 3), physical_seat, push_adjustment);
    }

    //handles script-specific things on completion of get up animation
    protected override void handleCompletedGetUp()
    {
        //update top right indicator and borders
        GetComponent<SecondaryScript>().onStationChange(SecondaryScript.DEFAULT_BORDER_CORDER, null);

        //update camera
        player_prefab.GetComponent<CameraMove>().parentRotationLock = false;
        float[] rotations = new float[] { 0.0f, 0.0f, 135.0f, 0.0f };
        player_prefab.GetComponent<CameraMove>().UnlockCamera(new Vector2(rotations[curr_seat], 30.0f));

        //clear occupancy
        ReferenceAssistor.Instance.seat_manager.clearSeatOccupancy(curr_seat);
    }

    //called when hitting shift while sitting down
    protected override void attemptSeatShift()
    {
        //captain doesn't shift
        if (curr_seat == 3)
        {
            return;
        }

        StartCoroutine(seatShift());
    }

    private IEnumerator seatShift()
    {
        ReferenceAssistor.Instance.seat_manager.beginShift(curr_seat);
        
        //trigger shift positional adjustments
        GameObject physical_seat = ReferenceAssistor.Instance.seat_manager.physical_seats[curr_seat];
        Vector2 push_adjustment = SeatManager.SEAT_PUSH_IN_ADJUSTMENTS[curr_seat];
        int new_seat_index = ReferenceAssistor.Instance.seat_manager.getShiftLocation(curr_seat, isLookingLeft());
        Vector3 end_shift_position = new Vector3(SeatManager.SEAT_COORDINATES[curr_seat][new_seat_index].x, physical_seat.transform.localPosition.y, SeatManager.SEAT_COORDINATES[curr_seat][new_seat_index].y);

        yield return player_prefab.GetComponent<PlayerMove>().SeatShift(physical_seat, push_adjustment, end_shift_position);

        ReferenceAssistor.Instance.seat_manager.updateSeatIndex(curr_seat, new_seat_index);
        onShiftChange();
    }

    protected override HUDInfo checkRayTarget()
    {
        int script_holder = curr_seat; //0 pilot, 1 tactician, 2 engineer, 3 captain
        if (current_ray_target.transform.childCount > 1)
        {
            script_holder = 4; //4 general modules
        }
        current_controllable = ReferenceAssistor.Instance.module_handlers[script_holder].GetComponent(current_ray_target.transform.GetChild(0).name) as IControllable;
        current_describable = ReferenceAssistor.Instance.module_handlers[script_holder].GetComponent(current_ray_target.transform.GetChild(0).name) as IDescribable;

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