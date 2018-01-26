// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Text;


namespace RSACrypto
{
  class ECTime
  {
  private DateTime UTCTime;
  private DateTime UTCStartScaleTime;
  // System.TimeZoneInfo



  public ECTime()
    {
    SetToYear1900();
    SetStartScaleTime();
    }




  internal ECTime( ulong Index )
    {
    SetFromIndex( Index );
    SetStartScaleTime();
    }



  internal void SetToNow()
    {
    UTCTime = DateTime.UtcNow;
    }



  internal void AddSeconds( double Seconds )
    {
    UTCTime = UTCTime.AddSeconds( Seconds );
    }



  internal void AddMinutes( int Minutes )
    {
    UTCTime = UTCTime.AddMinutes( Minutes );
    }



  internal DateTime GetAsLocalDateTime()
    {
    return UTCTime.ToLocalTime();
    }



  internal void SetFromLocalDateTime( DateTime SetFrom )
    {
    UTCTime = SetFrom.ToUniversalTime();
    }



  internal string GetLocalFileDayHourString()
    {
    DateTime RightNow = UTCTime.ToLocalTime();

    // string Result = RightNow.Year.ToString() + "_" +
    // string Result = RightNow.Month.ToString() +
    int Day = RightNow.Day;
    int Hour = RightNow.Hour % 3;
    string Result = Day.ToString() + "_" +
                    Hour.ToString();

    return Result;
    }



  internal int GetLocalYear()
    {
    return UTCTime.ToLocalTime().Year;
    }



  internal int GetLocalMonth()
    {
    return UTCTime.ToLocalTime().Month;
    }



  internal int GetLocalDay()
    {
    return UTCTime.ToLocalTime().Day;
    }



  internal int GetLocalHour()
    {
    return UTCTime.ToLocalTime().Hour;
    }



  internal int GetLocalMinute()
    {
    return UTCTime.ToLocalTime().Minute;
    }



  internal int GetLocalSecond()
    {
    return UTCTime.ToLocalTime().Second;
    }



  internal int GetYear()
    {
    return UTCTime.Year;
    } 



  internal int GetMonth()
    {
    return UTCTime.Month;
    } 



  internal int GetDay()
    {
    return UTCTime.Day;
    } 



  internal int GetHour()
    {
    return UTCTime.Hour;
    } 



  internal int GetMinute()
    {
    return UTCTime.Minute;
    } 



  internal int GetSecond()
    {
    return UTCTime.Second;
    } 



  internal int GetMillisecond()
    {
    return UTCTime.Millisecond;
    } 



  internal string ToLocalDateString()
    {
    return UTCTime.ToLocalTime().ToShortDateString();
    }




  internal string ToLocalDateStringShort()
    {
    DateTime RightNow = UTCTime.ToLocalTime();

    int Day = RightNow.Day;
    int Month = RightNow.Month;
    int Year = RightNow.Year;
    Year = Year % 100;

    string DayS = Day.ToString();
    if( DayS.Length == 1 )
      DayS = "0" + DayS;

    string MonthS = Month.ToString();
    if( MonthS.Length == 1 )
      MonthS = "0" + MonthS;

    string YearS = Year.ToString();
    if( YearS.Length == 1 )
      YearS = "0" + YearS;

    return MonthS + "/" + DayS + "/" + YearS;
    }



  internal string ToLocalDateStringVeryShort()
    {
    DateTime RightNow = UTCTime.ToLocalTime();

    int Day = RightNow.Day;
    int Month = RightNow.Month;

    string DayS = Day.ToString();
    if( DayS.Length == 1 )
      DayS = "0" + DayS;

    string MonthS = Month.ToString();
    if( MonthS.Length == 1 )
      MonthS = "0" + MonthS;

    return MonthS + "/" + DayS;
    }



  internal string ToLocalTimeString()
    {
    return UTCTime.ToLocalTime().ToShortTimeString();
    }



  internal void Copy( ECTime ToCopy )
    {
    // This won't be quite exact since it's to the nearest millisecond.

    UTCTime = new DateTime( ToCopy.GetYear(), 
                            ToCopy.GetMonth(),
                            ToCopy.GetDay(), 
                            ToCopy.GetHour(),
                            ToCopy.GetMinute(),
                            ToCopy.GetSecond(),
                            ToCopy.GetMillisecond(),
                            DateTimeKind.Utc ); // DateTimeKind.Local

    }




  internal void TruncateToEvenSeconds()
    {
    UTCTime = new DateTime( UTCTime.Year, 
                            UTCTime.Month,
                            UTCTime.Day, 
                            UTCTime.Hour,
                            UTCTime.Minute,
                            UTCTime.Second,
                            0,
                            DateTimeKind.Utc ); // DateTimeKind.Local

    }



  internal static ulong GetRandomishTickCount()
    {
    return (ulong)DateTime.UtcNow.Ticks;
    }



  internal ulong GetIndex()
    {
    // 16 bits is enough for the year 6,500 or so.  (64K)
    ulong Result = (uint)UTCTime.Year;

    Result <<= 4; // Room for a month up to 16.
    Result |= (uint)UTCTime.Month;

    Result <<= 5; // 32 days.
    Result |= (uint)UTCTime.Day;

    Result <<= 5; // 32 hours.
    Result |= (uint)UTCTime.Hour;

    Result <<= 6; // 64 minutes.
    Result |= (uint)UTCTime.Minute;

    Result <<= 6; // 64 seconds.
    Result |= (uint)UTCTime.Second;

    Result <<= 10;  // 1024 milliseconds.
    Result |= (uint)UTCTime.Millisecond;

    // 16 + 5 + 4 + 5 + 6 + 6 + 10.
    // (16 + 5) + (4 + 5) + (6 + 6) + 10.
    // 21 +          9 +      12    + 10
    // 30 +      22
    // 52 bits wide.

    return Result;
    }



  internal void SetToYear1900()
    {
    UTCTime = new DateTime( 1900, 
                            1,
                            1, 
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc ); // DateTimeKind.Local

    }




  internal void SetToYear2099()
    {
    UTCTime = new DateTime( 2099,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc); // DateTimeKind.Local

    }




  internal void SetToYear1999()
    {
    UTCTime = new DateTime( 1999,
                            1,
                            1,
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc); // DateTimeKind.Local

    }




  private void SetStartScaleTime()
    {
    UTCStartScaleTime = new DateTime( 1900,
                            1,
                            1, 
                            0,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc ); // DateTimeKind.Local

    }



  internal void SetFromIndex( ulong Index )
    {
    if( Index == 0 )
      {
      SetToYear1900();
      return;
      }

    // 10 bits
    int Millisecond = (int)(Index & 0x3FF);
    Index >>= 10;

    int Second = (int)(Index & 0x3F);
    Index >>= 6;

    int Minute = (int)(Index & 0x3F);
    Index >>= 6;

    int Hour = (int)(Index & 0x1F);
    Index >>= 5;

    int Day = (int)(Index & 0x1F);
    Index >>= 5;

    int Month = (int)(Index & 0xF);
    Index >>= 4;

    // 16 bits.
    int Year = (int)(Index & 0xFFFF);
    // Index >>= 16;

    if( Year <= 1900 )
      {
      SetToYear1900();
      return;
      }

    try
    {
    UTCTime = new DateTime( Year, 
                            Month,
                            Day, 
                            Hour,
                            Minute,
                            Second,
                            Millisecond,
                            DateTimeKind.Utc ); // DateTimeKind.Local

    }
    catch( Exception )
      {
      SetToYear1900();
      }
    }



  internal double GetDaysToNow()
    {
    DateTime RightNow = DateTime.UtcNow;
    TimeSpan TimeDif = RightNow.Subtract( UTCTime );
    return TimeDif.TotalDays;
    }



  internal double GetHoursToNow()
    {
    DateTime RightNow = DateTime.UtcNow;
    TimeSpan TimeDif = RightNow.Subtract( UTCTime );
    return TimeDif.TotalHours;
    }



  internal double GetMinutesToNow()
    {
    DateTime RightNow = DateTime.UtcNow;
    TimeSpan TimeDif = RightNow.Subtract( UTCTime );
    return TimeDif.TotalMinutes;
    }




  internal double GetSecondsToNow()
    {
    DateTime RightNow = DateTime.UtcNow;
    TimeSpan TimeDif = RightNow.Subtract( UTCTime );
    return TimeDif.TotalSeconds;
    }



  /*
  internal static string GetDayOfWeek( DateTime TheDate )
    {

    if( TheDate.DayOfWeek == DayOfWeek.Sunday )

      return "Sun";



    if( TheDate.DayOfWeek == DayOfWeek.Monday )

      return "Mon";



    if( TheDate.DayOfWeek == DayOfWeek.Tuesday )

      return "Tue";



    if( TheDate.DayOfWeek == DayOfWeek.Wednesday )

      return "Wed";



    if( TheDate.DayOfWeek == DayOfWeek.Thursday )

      return "Thu";



    if( TheDate.DayOfWeek == DayOfWeek.Friday )

      return "Fri";



    if( TheDate.DayOfWeek == DayOfWeek.Saturday )

      return "Sat";



    // Didn't find a match.

    return "";

    }

  */





  /*

  internal static string GetTimeString( DateTime TheTime )

    {

    int Hour = TheTime.Hour;

    string AmPm = "AM";

    if( Hour > 12 )

      {

      Hour -= 12;

      AmPm = "PM";

      }



    string HourS = Hour.ToString();

    if( HourS.Length == 1 )

      HourS = " " + HourS;

    

    string MinS = TheTime.Minute.ToString();

    if( MinS.Length == 1 )

      MinS = "0" + MinS;



    string SecS = TheTime.Second.ToString();

    if( SecS.Length == 1 )

      SecS = "0" + SecS;



    return HourS + ":" + MinS + ":" + SecS + " " + AmPm;

    }

  */



  internal void SetFromLocalValues( int Year,
                                    int Month,
                                    int Day,
                                    int Hour,
                                    int Minute,
                                    int Second )
    {
    if( Year <= 1900 )
      {
      SetToYear1900();
      return;
      }

    try
    {
    DateTime LocalTime = new DateTime( Year, 
                            Month,
                            Day, 
                            Hour,
                            Minute,
                            Second,
                            0,
                            DateTimeKind.Local );

    UTCTime = LocalTime.ToUniversalTime();
    }
    catch( Exception )
      {
      SetToYear1900();
      }
    }





  private string GetHeaderDayOfWeek()
    {
    if( UTCTime.DayOfWeek == DayOfWeek.Sunday )
      return "Sun";

    if( UTCTime.DayOfWeek == DayOfWeek.Monday )
      return "Mon";

    if( UTCTime.DayOfWeek == DayOfWeek.Tuesday )
      return "Tue";

    if( UTCTime.DayOfWeek == DayOfWeek.Wednesday )
      return "Wed";

    if( UTCTime.DayOfWeek == DayOfWeek.Thursday )
      return "Thu";

    if( UTCTime.DayOfWeek == DayOfWeek.Friday )
      return "Fri";

    if( UTCTime.DayOfWeek == DayOfWeek.Saturday )
      return "Sat";

    // It would match one of the days of the week, so it would never get here.

    return "Nada";
    }




  private string GetMonthAsString()
    {
    int Month = UTCTime.Month;

    switch( Month )
      {
      case 1: return "Jan";
      case 2: return "Feb";
      case 3: return "Mar";
      case 4: return "Apr";
      case 5: return "May";
      case 6: return "Jun";
      case 7: return "Jul";
      case 8: return "Aug";
      case 9: return "Sep";
      case 10: return "Oct";
      case 11: return "Nov";
      case 12: return "Dec";
      }

    return "Nada";
    }




  internal string GetHTTPHeaderDateTime()
    {
    // Hypertext Transfer Protocol:
    // http://www.ietf.org/rfc/rfc2616.txt

    // From RFC 2616:
    // "HTTP applications have historically allowed three different formats
    // for the representation of date/time stamps:
    // Sun, 06 Nov 1994 08:49:37 GMT  ; RFC 822, updated by RFC 1123
    // Sunday, 06-Nov-94 08:49:37 GMT ; RFC 850, obsoleted by RFC 1036
    // Sun Nov  6 08:49:37 1994       ; ANSI C's asctime() format
    // The first format is preferred as an Internet standard."

    // So it should look like this:
    // Sun, 06 Nov 1994 08:49:37 GMT

    // This is what the Apache server is sending back:
    // Sun, 27 Jul 2014 18:16:29 GMT
    string DayOfWeek = GetHeaderDayOfWeek();

    int DayOfMonth = UTCTime.Day;
    string MonthName = GetMonthAsString();

    int Year = UTCTime.Year;
    int Day = UTCTime.Day;
    int Hour = UTCTime.Year;
    int Minute = UTCTime.Year;
    int Second = UTCTime.Year;

    string DayS = Day.ToString();
    if( DayS.Length == 1 )
      DayS = "0" + DayS;

    string HourS = Hour.ToString();
    if( HourS.Length == 1 )
      HourS = "0" + HourS;

    string MinuteS = Minute.ToString();
    if( MinuteS.Length == 1 )
      MinuteS = "0" + MinuteS;

    string SecondS = Second.ToString();
    if( SecondS.Length == 1 )
      SecondS = "0" + SecondS;

    // Sun, 27 Jul 2014 18:16:29 GMT
    string Result = DayOfWeek + ", " +
                    DayS + " " +
                    MonthName + " " +
                    Year.ToString() + " " +
                    HourS + ":" +
                    MinuteS + ":" +
                    SecondS +
                    " GMT";

    return Result;
    }





  internal string GetLocalMillisecondTime()
    {
    DateTime RightNow = UTCTime.ToLocalTime();

    // This won't be quite exact since it's to the nearest millisecond.
    string HourS = RightNow.Hour.ToString();
    string MinuteS = RightNow.Minute.ToString();
    string SecondS = RightNow.Second.ToString();
    string MillisecS = RightNow.Millisecond.ToString();

    if( MinuteS.Length == 1 )
      MinuteS = "0" + MinuteS;

    if( SecondS.Length == 1 )
      SecondS = "0" + SecondS;

    if( MillisecS.Length == 1 )
      MillisecS = "0" + MillisecS;

    if( MillisecS.Length == 2 )
      MillisecS = "0" + MillisecS;

    string Result = HourS + ":" +
                    MinuteS + ":" +
                    SecondS + ":" +
                    MillisecS;

    return Result;
    }




  internal ulong ToUnixTime()
    {
    DateTime StartTime = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
    TimeSpan Diff = UTCTime - StartTime;
    return (ulong)Diff.TotalSeconds;
    }




  }
}
