/*
    WaveInfo.cs
    - Holds information about a specific scan wave (ex. colors, number of rings, shapes, etc.)
    Contributor(s): Jake Schott
    Last Updated: 6/8/2025
*/

using UnityEngine;

public class WaveInfo
{
    private bool is_contracted = false;

    private Texture center;
    private Color center_color;
    private float center_speed;

    private int number_of_rings = 0;
    private Color[] ring_colors = null;
    private Texture[] ring_textures = null;
    private bool[] ring_is_solid = null;
    private float[] ring_speeds = null;

    public void setCenter(Texture c, Color c_color, float speed)
    {
        center = c;
        center_color = c_color;
        center_speed = speed;
    }

    public void setRings(int num, Color[] colors, Texture[] textures, bool[] is_solid, float[] speeds)
    {
        number_of_rings = num;
        ring_colors = colors;
        ring_textures = textures;
        ring_speeds = speeds;
        ring_is_solid = is_solid;
    }

    public Texture getCenterTexture()
    {
        return center;
    }

    public Color getCenterColor()
    {
        return center_color;
    }

    public float getCenterSpeed()
    {
        return center_speed;
    }

    public int getNumberOfRings()
    {
        return number_of_rings;
    }

    public Color[] getRingColors()
    {
        return ring_colors;
    }

    public Texture[] getRingTextures()
    {
        return ring_textures;
    }

    public bool[] getRingSolids()
    {
        return ring_is_solid;
    }

    public float[] getRingSpeeds()
    {
        return ring_speeds;
    }
}
