using UnityEngine;
//TODO: Remove hours and just use minutes in range 1 to 1440.

/// <summary>
/// Game clock for the world.
/// </summary>
/// Ticks up the game clock one minute every [clockrate] seconds.
public class GameClock : UWEBase
{
    public static int Clock0
    {
        get
        {
            if (_RES== GAME_UW2)
            {
                return SaveGame.GetAt(0x36A);
            }
            return SaveGame.GetAt(0xCF);
        }
        set
        {
            if (value > 255) { Clock1 += (value - 255); value = 0; }
            if (_RES == GAME_UW2)
            {
                SaveGame.SetAt(0x36A,(byte)value);
                return;
            }
           SaveGame.SetAt(0xCF,(byte)value);
        }
    }
    public static int Clock1
    {
        get
        {
            if (_RES == GAME_UW2)
            {
                return SaveGame.GetAt(0x36B);
            }
            return SaveGame.GetAt(0xD0);
        }
        set
        {
            if (value > 255) { Clock2+=(value - 255); value = 0; }
            if (_RES == GAME_UW2)
            {
                SaveGame.SetAt(0x36B, (byte)value);
                return;
            }
            SaveGame.SetAt(0xD0, (byte)value);
        }
    }
    public static int Clock2
    {
        get
        {
            if (_RES == GAME_UW2)
            {
                return SaveGame.GetAt(0x36C);
            }
            return SaveGame.GetAt(0xD1);
        }
        set
        {
            if (value > 255) { Clock3 += (value - 255); value = 0; }
            if (_RES == GAME_UW2)
            {
                SaveGame.SetAt(0x36C, (byte)value);
                return;
            }
            SaveGame.SetAt(0xD1, (byte)value);
        }
    }
    public static int Clock3
    {
        get
        {
            if (_RES == GAME_UW2)
            {
                return SaveGame.GetAt(0x36d);
            }
            return SaveGame.GetAt(0xD2);
        }
        set
        {
            if (value > 255) { Clock0=0; Clock1 = 0;Clock2 = 0; Clock3 = 0; Debug.Log("WOW The clocks overflowed!"); }//Overflow
            if (_RES == GAME_UW2)
            {
                SaveGame.SetAt(0x36D, (byte)value);
                return;
            }
            SaveGame.SetAt(0xD2, (byte)value);
        }
    }

    public static int TotalSeconds
    {
        get
        {
            return Clock1 + (Clock2 * 255) + (Clock3 * 255 * 255);
        }
    }
    public static int Hour
    {
        get
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(TotalSeconds);
            return t.Hours;
        }
    }

    public static int Second
    {
        get
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(TotalSeconds);
            return t.Seconds;
        }
    }
    public static int Minute
    {
        get
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(TotalSeconds);
            return t.Minutes;
        }
    }
    public static int Day
    {
        get
        {
            System.TimeSpan t = System.TimeSpan.FromSeconds(TotalSeconds);
            return (int)t.TotalDays;
        }
    }

    public int[] gametimevals = new int[3]; //For save games.

  //  public int game_time;

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
        if (clockTime >= clockRate)
        {
            Clock1++;
            clockTime = 0.0f;
            SecondUpdate();
            if (Second % 60 == 0)
            {
                EveryMinuteUpdate();//Move minute forward
            }
            if (Second % 30 == 0)
            {
                EveryHalfMinuteUpdate();//Move minute forward
            }
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
    public static void Advance(int minutestoadvance=1)
    {
        for (int i = 0; i < minutestoadvance; i++)
        {
            Clock1 += 60;//Move forward 60 seconds
            EveryMinuteUpdate();//Do the once a minute update
            //Do two 30 seconds updates
            EveryHalfMinuteUpdate();
            EveryHalfMinuteUpdate();
        }
    }
}
