using UnityEngine;

/// <summary>
/// Game clock for the world.
/// </summary>
/// Ticks up the game clock one minute every [clockrate] seconds.
public class GameClock : UWEBase
{
    private float totalClock = 0;
    //public static int Clock0
    //{
    //    get
    //    {
    //        if (_RES== GAME_UW2)
    //        {
    //            return SaveGame.GetAt(0x36A);
    //        }
    //        return SaveGame.GetAt(0xCF);
    //    }
    //    set
    //    {
    //        if (value > 255) { Clock1 += (value - 255); value = 0; }
    //        if (_RES == GAME_UW2)
    //        {
    //            SaveGame.SetAt(0x36A,(byte)value);
    //            return;
    //        }
    //       SaveGame.SetAt(0xCF,(byte)value);
    //    }
    //}
    //public static int Clock1
    //{
    //    get
    //    {
    //        if (_RES == GAME_UW2)
    //        {
    //            return SaveGame.GetAt(0x36B);
    //        }
    //        return SaveGame.GetAt(0xD0);
    //    }
    //    set
    //    {
    //        if (value > 255) { Clock2+=(value - 255); value = 0; }
    //        if (_RES == GAME_UW2)
    //        {
    //            SaveGame.SetAt(0x36B, (byte)value);
    //            return;
    //        }
    //        SaveGame.SetAt(0xD0, (byte)value);
    //    }
    //}
    //public static int Clock2
    //{
    //    get
    //    {
    //        if (_RES == GAME_UW2)
    //        {
    //            return SaveGame.GetAt(0x36C);
    //        }
    //        return SaveGame.GetAt(0xD1);
    //    }
    //    set
    //    {
    //        if (value > 255) { Clock3 += (value - 255); value = 0; }
    //        if (_RES == GAME_UW2)
    //        {
    //            SaveGame.SetAt(0x36C, (byte)value);
    //            return;
    //        }
    //        SaveGame.SetAt(0xD1, (byte)value);
    //    }
    //}
    //public static int Clock3
    //{
    //    get
    //    {
    //        if (_RES == GAME_UW2)
    //        {
    //            return SaveGame.GetAt(0x36d);
    //        }
    //        return SaveGame.GetAt(0xD2);
    //    }
    //    set
    //    {
    //        if (value > 255) { Clock0=0; Clock1 = 0;Clock2 = 0; Clock3 = 0; Debug.Log("WOW The clocks overflowed!"); }//Overflow
    //        if (_RES == GAME_UW2)
    //        {
    //            SaveGame.SetAt(0x36D, (byte)value);
    //            return;
    //        }
    //        SaveGame.SetAt(0xD2, (byte)value);
    //    }
    //}

    //public static int TotalSeconds
    //{
    //    get
    //    {
    //        return Clock1 + (Clock2 * 255) + (Clock3 * 255 * 255);
    //    }
    //}

    /// <summary>
    /// Hour for pocket watch
    /// </summary>
    public static int TwelveHourClock
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }

            return (ClockValue / 0xE1000) % 12;
        }
    }

    /// <summary>
    /// Days for end game screen
    /// </summary>
    /// Note the calc is different from game_days that is used in conversations.
    public static int days
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }
            return (ClockValue / 0x1C2000) / 12;
        }
    }

    /// <summary>
    /// Minute for pocket watch
    /// </summary>
    public static int Minute
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }
            return (ClockValue / 0x3C00) % 60;
        }
    }
    /// <summary>
    /// For conversation variable
    /// </summary>
    public static int game_days
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }
            return (ClockValue / 0x1502e80);
        }
    }

    public int[] gametimevals = new int[3]; //For save games.


    /// <summary>
    /// Game time for conversation variable
    /// </summary>
    public static int game_time
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }
            return (ClockValue / 0x3BC4);
        }
    }


    /// <summary>
    /// Game minute value for conversation variable
    /// </summary>
    public static int game_mins
    {
        get
        {
            int ClockValue;
            if (_RES == GAME_UW2)
            {
                ClockValue = SaveGame.GetAt32(0x36A);
            }
            else
            {
                ClockValue = SaveGame.GetAt32(0xCF);
            }
            return (ClockValue / 0x3BC4) % 0x5A0;
        }
    }

    /// <summary>
    /// How long has passed since the last clock tick
    /// </summary>
    public float clockTime;
    /// <summary>
    /// The clock rate. How long is a second relative to the clockTime
    /// </summary>
    public float clockRate = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if (ConversationVM.InConversation) { return; }
        if (GameWorldController.instance.AtMainMenu){ return; }
        clockTime += Time.deltaTime;
        totalClock += Time.deltaTime;
        if (clockTime >= clockRate)
        {
            //Clock1++;//advance internal game time 1 second against real world time

            ClockIncrement(1);
            clockTime = 0.0f;
            //SecondUpdate();
            //if (Second % 60 == 0)
            //{
            //    EveryMinuteUpdate();//Move minute forward
            //}
            //if (Second % 30 == 0)
            //{
            //    EveryHalfMinuteUpdate();//Move minute forward
            //}
        }
        if (totalClock>=30)
        {
            //TODO: Rework updates to be more in line with actual behaviour.
            EveryMinuteUpdate();
            EveryHalfMinuteUpdate();
            totalClock = 0f;
        }
    }


    /// <summary>
    /// Increment the clock without making any other gameplay changes.
    /// </summary>
    /// <param name="increment"></param>
    public static void ClockIncrement(int increment)
    {
        int ClockValue;
        if (_RES == GAME_UW2)
        {
            ClockValue = SaveGame.GetAt32(0x36A);
            SaveGame.SetAt32(0x36A, ClockValue + increment);
        }
        else
        {
            ClockValue = SaveGame.GetAt32(0xCF);
            SaveGame.SetAt32(0xCF, ClockValue + increment);
        }       
    }


    public static void ClockSet(int newTime)
    {
        int ClockValue;
        if (_RES == GAME_UW2)
        {
            ClockValue = SaveGame.GetAt32(0x36A);
            SaveGame.SetAt32(0x36A,  newTime);
        }
        else
        {
            ClockValue = SaveGame.GetAt32(0xCF);
            SaveGame.SetAt32(0xCF,  newTime);
        }
    }


    /// <summary>
    /// Updates that happen every second.
    /// </summary>
    public static void SecondUpdate()
    {
        if(_RES==GAME_UW2)
        {//Paralysed player only happens in UW2 (as far as I know)
            if (UWCharacter.Instance.ParalyzeTimer > 0)
            {
                UWCharacter.Instance.ParalyzeTimer--;
            }
        }
    }

    /// <summary>
    /// Clock tick for every minute
    /// </summary>
    static void EveryMinuteUpdate()
    {//Advance the time.
       // Debug.Log("Minute update");
        UWCharacter.Instance.RegenMana();
        UWCharacter.Instance.UpdateHungerAndFatigue();
    }
    
    static void EveryHalfMinuteUpdate()
    {
        //Poison updates every 30 seconds.
        UWCharacter.Instance.PoisonUpdate();
        if(UWCharacter.Instance.Intoxication>=3)
        {//Sober up over time.
            UWCharacter.Instance.Intoxication -= 3;
        }
        else 
        { 
            UWCharacter.Instance.Intoxication = 0; 
        }
    }

    /// <summary>
    /// Move the clock by specified no of minutes
    /// </summary>
    public static void Advance(int minutestoadvance=60)
    {
        for (int i = 0; i < minutestoadvance; i++)
        {
            // Clock1 += 60;//Move forward 60 seconds???
            GameClock.ClockIncrement(60);
            EveryMinuteUpdate();//Do the once a minute update
            //Do two 30 seconds updates
            EveryHalfMinuteUpdate();
            EveryHalfMinuteUpdate();
        }
    }
}
