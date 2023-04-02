using System;

public class FeeInterval
{
    public TimeOnly Start { get; init; }
    public TimeOnly End { get; init; }
    public int Fee { get; init; }

    public FeeInterval(int hourStart, int minStart, int hourEnd, int minEnd, int fee)
    {
        Start = new TimeOnly(hourStart, minStart);
        if (hourEnd == 24)
        {
            End = TimeOnly.MaxValue;
        }
        else
        {
            End = new TimeOnly(hourEnd, minEnd);
        }
        Fee = fee;
    }

    public bool Contains(TimeOnly point)
    {
        return point >= Start && point <= End;
    }
}