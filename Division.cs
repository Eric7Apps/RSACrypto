// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


// These ideas mainly come from a set of books
// written by Donald Knuth in the 1960s called
// "The Art of Computer Programming", especially
// Volume 2  Seminumerical Algorithms.

// But it is also a whole lot like the ordinary
// arithmetic you'd do on paper, and there are
// some comments in the code below about that.

// For more on division see also:
// Brinch Hansen, Multiple-Length Division Revisited,
// 1994
// http://brinch-hansen.net/papers/1994b.pdf


using System;
using System.Text;
// using System.ComponentModel; // BackgroundWorker


namespace RSACrypto
{
  class Division
  {
  private MainForm MForm;
  private IntegerMath IntMath;

  // These numbers are created ahead of time so that
  // they don't have to be created over and over
  // again within a loop where the calculations are
  // being done.
  private Integer ToDivide = new Integer();
  private Integer Quotient = new Integer();
  private Integer Remainder = new Integer();
  private Integer ToDivideKeep = new Integer();
  private Integer DivideByKeep = new Integer();
  private Integer DivideBy = new Integer();
  private Integer TestForBits = new Integer();
  private Integer TestForDivide1 = new Integer();
  // private bool Cancelled = false;


  private Division()
    {
    }



  internal Division( MainForm UseForm, IntegerMath UseIntMath )
    {
    MForm = UseForm;
    IntMath = UseIntMath;

    // You might want to pass a null IntMath to this
    // so that it creates its own that doesn't
    // interfere with something else.
    if( IntMath == null )
      IntMath = new IntegerMath( MForm );

    }


  private void ShowStatus( string ToShow )
    {
    if( MForm == null )
      return;

    MForm.ShowStatus( ToShow );
    }



  private bool ShortDivide( Integer ToDivide,
                            Integer DivideBy,
                            Integer Quotient,
                            Integer Remainder )
    {
    Quotient.Copy( ToDivide );
    // DivideBy has an Index of zero:
    ulong DivideByU = DivideBy.GetD( 0 );
    ulong RemainderU = 0;
    // Get the first one set up.
    if( DivideByU > Quotient.GetD( Quotient.GetIndex()) )
      {
      Quotient.SetD( Quotient.GetIndex(), 0 );
      }
    else
      {
      ulong OneDigit = Quotient.GetD( Quotient.GetIndex() );
      Quotient.SetD( Quotient.GetIndex(), OneDigit / DivideByU );
      RemainderU = OneDigit % DivideByU;
      ToDivide.SetD( ToDivide.GetIndex(), RemainderU );
      }

    // Now do the rest.
    for( int Count = Quotient.GetIndex(); Count >= 1; Count-- )
      {
      ulong TwoDigits = ToDivide.GetD( Count );
      TwoDigits <<= 32;
      TwoDigits |= ToDivide.GetD( Count - 1 );
      Quotient.SetD( Count - 1, TwoDigits / DivideByU );
      RemainderU = TwoDigits % DivideByU;
      ToDivide.SetD( Count, 0 );
      ToDivide.SetD( Count - 1, RemainderU ); // What's left to divide.
      }

    // Set the index for the quotient.
    // The quotient would have to be at least 1 here,
    // so it will find where to set the index.
    for( int Count = Quotient.GetIndex(); Count >= 0; Count-- )
      {
      if( Quotient.GetD( Count ) != 0 )
        {
        Quotient.SetIndex( Count );
        break;
        }
      }

    Remainder.SetD( 0, RemainderU );
    Remainder.SetIndex( 0 );
    if( RemainderU == 0 )
      return true;
    else
      return false;

    }



  // This is a variation on ShortDivide that returns the remainder.
  // Also, DivideBy is a ulong.
  internal ulong ShortDivideRem( Integer ToDivideOriginal,
                               ulong DivideByU,
                               Integer Quotient )
    {
    if( ToDivideOriginal.IsULong())
      {
      ulong ToDiv = ToDivideOriginal.GetAsULong();
      ulong Q = ToDiv / DivideByU;
      Quotient.SetFromULong( Q );
      return ToDiv % DivideByU;
      }

    ToDivide.Copy( ToDivideOriginal );
    Quotient.Copy( ToDivide );
    ulong RemainderU = 0;
    if( DivideByU > Quotient.GetD( Quotient.GetIndex() ))
      {
      Quotient.SetD( Quotient.GetIndex(), 0 );
      }
    else
      {
      ulong OneDigit = Quotient.GetD( Quotient.GetIndex() );
      Quotient.SetD( Quotient.GetIndex(), OneDigit / DivideByU );
      RemainderU = OneDigit % DivideByU;
      ToDivide.SetD( ToDivide.GetIndex(), RemainderU );
      }

    for( int Count = Quotient.GetIndex(); Count >= 1; Count-- )
      {
      ulong TwoDigits = ToDivide.GetD( Count );
      TwoDigits <<= 32;
      TwoDigits |= ToDivide.GetD( Count - 1 );
      Quotient.SetD( Count - 1, TwoDigits / DivideByU );
      RemainderU = TwoDigits % DivideByU;
      ToDivide.SetD( Count, 0 );
      ToDivide.SetD( Count - 1, RemainderU );
      }

    for( int Count = Quotient.GetIndex(); Count >= 0; Count-- )
      {
      if( Quotient.GetD( Count ) != 0 )
        {
        Quotient.SetIndex( Count );
        break;
        }
      }

    return RemainderU;
    }



  // This is a variation on ShortDivide() to get the
  // remainder only.
  internal ulong GetMod32( Integer ToDivideOriginal, ulong DivideByU )
    {
    if( (DivideByU >> 32) != 0 )
      throw( new Exception( "GetMod32: (DivideByU >> 32) != 0." ));

    // If this is _equal_ to a small prime it would return zero.
    if( ToDivideOriginal.IsULong())
      {
      ulong Result = ToDivideOriginal.GetAsULong();
      return Result % DivideByU;
      }

    ToDivide.Copy( ToDivideOriginal );
    ulong RemainderU = 0;
    if( DivideByU <= ToDivide.GetD( ToDivide.GetIndex() ))
      {
      ulong OneDigit = ToDivide.GetD( ToDivide.GetIndex() );
      RemainderU = OneDigit % DivideByU;
      ToDivide.SetD( ToDivide.GetIndex(), RemainderU );
      }

    for( int Count = ToDivide.GetIndex(); Count >= 1; Count-- )
      {
      ulong TwoDigits = ToDivide.GetD( Count );
      TwoDigits <<= 32;
      TwoDigits |= ToDivide.GetD( Count - 1 );
      RemainderU = TwoDigits % DivideByU;
      ToDivide.SetD( Count, 0 );
      ToDivide.SetD( Count - 1, RemainderU );
      }

    return RemainderU;
    }



   /*
  private bool EquivalentMod()
    {
    // The compiler does a compile-time check on these constants
    // and it finds unreachable code because they are not false.
    const uint Test1 = ((2 * 10) + 3) % 15;
    const uint Test2 = (((2 * 10) % 15) + (3 % 15)) % 15;
    const uint Test3 = ((((2 % 15) * (10 % 15)) % 15) + (3 % 15)) % 15;
    if( Test1 == Test2 )
      {
      if( Test2 == Test3 )
        return true;
      else
        return false; // Unreachable code.

      }
    else
      return false; // Unreachable code.

    // The compiler only tells you about the first unreachable code it
    // finds.  Comment out earlier ones to make it show later ones.
    }
    */



  private ulong GetMod64FromTwoULongs( ulong P1, ulong P0, ulong Divisor64 )
    {
    if( Divisor64 <= 0xFFFFFFFF )
      throw( new Exception( "GetMod64FromTwoULongs Divisor64 <= 0xFFFFFFFF" ));

    // This is never shifted more than 12 bits, so
    // check to make sure there's room to shift it.
    if( (Divisor64 >> 52) != 0 )
      throw( new Exception( "Divisor64 is too big in GetMod64FromTwoULongs." ));

    if( P1 == 0 )
      return P0 % Divisor64;

    //////////////////////////////////////////////
    // R ~ (a*b) mod m
    // R ~ ((a mod m) * (b mod m)) mod m
    // (P1 * 2^64) + P0 is what the number is.
    ulong Part1 = P1 % Divisor64;
    if( (Divisor64 >> 40) == 0 )
      {
      // Then this can be done 24 bits at a time.
      Part1 <<= 24;  // Times 2^24
      Part1 = Part1 % Divisor64;
      Part1 <<= 24;  //  48
      Part1 = Part1 % Divisor64;
      Part1 <<= 16;  // Brings it to 64
      Part1 = Part1 % Divisor64;
      }
    else
      {
      Part1 <<= 12;  // Times 2^12
      Part1 = Part1 % Divisor64;
      Part1 <<= 12;  // Times 2^12
      Part1 = Part1 % Divisor64;
      Part1 <<= 12;  // Times 2^12
      Part1 = Part1 % Divisor64;
      Part1 <<= 12;  // Times 2^12 Brings it to 48.
      Part1 = Part1 % Divisor64;
      Part1 <<= 8;  // Times 2^8
      Part1 = Part1 % Divisor64;
      Part1 <<= 8;  // Times 2^8 Brings it to 64.
      Part1 = Part1 % Divisor64;
      }

    // All of the above was just to get the P1 part
    // of it, so now add P0:
    return (Part1 + P0) % Divisor64;
    }



  internal ulong GetMod64( Integer ToDivideOriginal, ulong DivideBy )
    {
    if( ToDivideOriginal.IsULong())
      return ToDivideOriginal.GetAsULong() % DivideBy;

    ToDivide.Copy( ToDivideOriginal );
    ulong Digit1;
    ulong Digit0;
    ulong Remainder;
    if( ToDivide.GetIndex() == 2 )
      {
      Digit1 = ToDivide.GetD( 2 );
      Digit0 = ToDivide.GetD( 1 ) << 32;
      Digit0 |= ToDivide.GetD( 0 );
      return GetMod64FromTwoULongs( Digit1, Digit0, DivideBy );
      }

    if( ToDivide.GetIndex() == 3 )
      {
      Digit1 = ToDivide.GetD( 3 ) << 32;
      Digit1 |= ToDivide.GetD( 2 );
      Digit0 = ToDivide.GetD( 1 ) << 32;
      Digit0 |= ToDivide.GetD( 0 );
      return GetMod64FromTwoULongs( Digit1, Digit0, DivideBy );
      }

    int Where = ToDivide.GetIndex();
    while( true )
      {
      if( Where <= 3 )
        {
        if( Where < 2 ) // This can't happen.
          throw( new Exception( "GetMod64(): Where < 2." ));

        if( Where == 2 )
          {
          Digit1 = ToDivide.GetD( 2 );
          Digit0 = ToDivide.GetD( 1 ) << 32;
          Digit0 |= ToDivide.GetD( 0 );
          return GetMod64FromTwoULongs( Digit1, Digit0, DivideBy );
          }

        if( Where == 3 )
          {
          Digit1 = ToDivide.GetD( 3 ) << 32;
          Digit1 |= ToDivide.GetD( 2 );
          Digit0 = ToDivide.GetD( 1 ) << 32;
          Digit0 |= ToDivide.GetD( 0 );
          return GetMod64FromTwoULongs( Digit1, Digit0, DivideBy );
          }
        }
      else
        {
        // The index is bigger than 3.
        // This part would get called at least once.
        Digit1 = ToDivide.GetD( Where ) << 32;
        Digit1 |= ToDivide.GetD( Where - 1 );
        Digit0 = ToDivide.GetD( Where - 2 ) << 32;
        Digit0 |= ToDivide.GetD( Where - 3 );
        Remainder = GetMod64FromTwoULongs( Digit1, Digit0, DivideBy );
        ToDivide.SetD( Where, 0 );
        ToDivide.SetD( Where - 1, 0 );
        ToDivide.SetD( Where - 2, Remainder >> 32 );
        ToDivide.SetD( Where - 3, Remainder & 0xFFFFFFFF );
        }

      Where -= 2;
      }
    }




  internal void Divide( Integer ToDivideOriginal,
                        Integer DivideByOriginal,
                        Integer Quotient,
                        Integer Remainder )
    {
    if( ToDivideOriginal.IsNegative )
      throw( new Exception( "Divide() can't be called with negative numbers." ));

    if( DivideByOriginal.IsNegative )
      throw( new Exception( "Divide() can't be called with negative numbers." ));

    // Returns true if it divides exactly with zero remainder.
    // This first checks for some basics before trying to divide it:
    if( DivideByOriginal.IsZero() )
      throw( new Exception( "Divide() dividing by zero." ));

    ToDivide.Copy( ToDivideOriginal );
    DivideBy.Copy( DivideByOriginal );
    if( ToDivide.ParamIsGreater( DivideBy ))
      {
      Quotient.SetToZero();
      Remainder.Copy( ToDivide );
      return; //  false;
      }

    if( ToDivide.IsEqual( DivideBy ))
      {
      Quotient.SetFromULong( 1 );
      Remainder.SetToZero();
      return; //  true;
      }

    // At this point DivideBy is smaller than ToDivide.
    if( ToDivide.IsULong() )
      {
      ulong ToDivideU = ToDivide.GetAsULong();
      ulong DivideByU = DivideBy.GetAsULong();
      ulong QuotientU = ToDivideU / DivideByU;
      ulong RemainderU = ToDivideU % DivideByU;
      Quotient.SetFromULong( QuotientU );
      Remainder.SetFromULong( RemainderU );
      // if( RemainderU == 0 )
        return; //  true;
      // else
        // return false;

      }

    if( DivideBy.GetIndex() == 0 )
      {
      ShortDivide( ToDivide, DivideBy, Quotient, Remainder );
      return;
      }

    // return LongDivide1( ToDivide, DivideBy, Quotient, Remainder );
    // return LongDivide2( ToDivide, DivideBy, Quotient, Remainder );
    LongDivide3( ToDivide, DivideBy, Quotient, Remainder );
    }




  /*
  private bool LongDivide1( Integer ToDivide,
                            Integer DivideBy,
                            Integer Quotient,
                            Integer Remainder )
    {
    Integer Test1 = new Integer();
    int TestIndex = ToDivide.Index - DivideBy.Index;
    if( TestIndex != 0 )
      {
      // Is 1 too high?
      Test1.SetDigitAndClear( TestIndex, 1 );
      Test1.MultiplyTopOne( DivideBy );
      if( ToDivide.ParamIsGreater( Test1 ))
        TestIndex--;

      }

    Quotient.SetDigitAndClear( TestIndex, 1 );
    Quotient.D[TestIndex] = 0;
    uint BitTest = 0x80000000;
    while( true )
      {
      // For-loop to test each bit:
      for( int BitCount = 31; BitCount >= 0; BitCount-- )
        {
        Test1.Copy( Quotient );
        Test1.D[TestIndex] |= BitTest;
        Test1.Multiply( DivideBy );
        if( Test1.ParamIsGreaterOrEq( ToDivide ))
          Quotient.D[TestIndex] |= BitTest; // Then keep the bit.

        BitTest >>= 1;
        }

      if( TestIndex == 0 )
        break;

      TestIndex--;
      BitTest = 0x80000000;
      }

    Test1.Copy( Quotient );
    Test1.Multiply( DivideBy );
    if( Test1.IsEqual( ToDivide ) )
      {
      Remainder.SetToZero();
      return true; // Divides exactly.
      }

    Remainder.Copy( ToDivide );
    Remainder.Subtract( Test1 );
    // Does not divide it exactly.
    return false;
    }
    */



  private void TestDivideBits( ulong MaxValue,
                               bool IsTop,
                               int TestIndex,
                               Integer ToDivide,
                               Integer DivideBy,
                               Integer Quotient,
                               Integer Remainder )
    {
    // For a particular value of TestIndex, this does the
    // for-loop to test each bit.
    // When you're not testing you wouldn't want to be creating these
    // and allocating the RAM for them each time it's called.

    uint BitTest = 0x80000000;
    for( int BitCount = 31; BitCount >= 0; BitCount-- )
      {
      if( (Quotient.GetD( TestIndex ) | BitTest) > MaxValue )
        {
        // If it's more than the MaxValue then the
        // multiplication test can be skipped for
        // this bit.
        // SkippedMultiplies++;
        BitTest >>= 1;
        continue;
        }

      // Is it only doing the multiplication for the top digit?
      if( IsTop )
        {
        TestForBits.Copy( Quotient );
        TestForBits.SetD( TestIndex, TestForBits.GetD( TestIndex ) | BitTest );
        IntMath.MultiplyTop( TestForBits, DivideBy );
        }
      else
        {
        TestForBits.Copy( Quotient );
        TestForBits.SetD( TestIndex, TestForBits.GetD( TestIndex ) | BitTest );
        IntMath.Multiply( TestForBits, DivideBy );
        }

      if( TestForBits.ParamIsGreaterOrEq( ToDivide ))
        Quotient.SetD( TestIndex, Quotient.GetD( TestIndex ) | BitTest ); // Keep the bit.

      BitTest >>= 1;
      }
    }




  /*
  // This works like LongDivide1 except that it estimates the maximum
  // value for the digit and the for-loop for bit testing is called
  // as a separate function.
  private bool LongDivide2( Integer ToDivide,
                            Integer DivideBy,
                            Integer Quotient,
                            Integer Remainder )
    {
    Integer Test1 = new Integer();
    int TestIndex = ToDivide.Index - DivideBy.Index;
    // See if TestIndex is too high.
    if( TestIndex != 0 )
      {
      // Is 1 too high?
      Test1.SetDigitAndClear( TestIndex, 1 );
      Test1.MultiplyTopOne( DivideBy );
      if( ToDivide.ParamIsGreater( Test1 ))
        TestIndex--;
      }

    // If you were multiplying 99 times 97 you'd get 9,603 and the upper
    // two digits [96] are used to find the MaxValue.  But if you were multiply
    // 12 * 13 you'd have 156 and only the upper one digit is used to find
    // the MaxValue.
    // Here it checks if it should use one digit or two:
    ulong MaxValue;
    if( (ToDivide.Index - 1) > (DivideBy.Index + TestIndex) )
      {
      MaxValue = ToDivide.D[ToDivide.Index];
      }
    else
      {
      MaxValue = ToDivide.D[ToDivide.Index] << 32;
      MaxValue |= ToDivide.D[ToDivide.Index - 1];
      }

    MaxValue = MaxValue / DivideBy.D[DivideBy.Index];
    Quotient.SetDigitAndClear( TestIndex, 1 );
    Quotient.D[TestIndex] = 0;
    TestDivideBits( MaxValue,
                    true,
                    TestIndex,
                    ToDivide,
                    DivideBy,
                    Quotient,
                    Remainder );

    if( TestIndex == 0 )
      {
      Test1.Copy( Quotient );
      Test1.Multiply( DivideBy );
      Remainder.Copy( ToDivide );
      Remainder.Subtract( Test1 );
      ///////////////
      if( DivideBy.ParamIsGreater( Remainder ))
        throw( new Exception( "Remainder > DivideBy in LongDivide2()." ));

      //////////////
      if( Remainder.IsZero() )
        return true;
      else
        return false;

      }

    TestIndex--;
    while( true )
      {
      // This remainder is used the same way you do long division
      // with paper and pen and you keep working with a remainder
      // until the remainder is reduced to something smaller than
      // DivideBy.  You look at the remainder to estimate
      // your next quotient digit.
      Test1.Copy( Quotient );
      Test1.Multiply( DivideBy );
      Remainder.Copy( ToDivide );
      Remainder.Subtract( Test1 );
      MaxValue = Remainder.D[Remainder.Index] << 32;
      MaxValue |= Remainder.D[Remainder.Index - 1];
      MaxValue = MaxValue / DivideBy.D[DivideBy.Index];
      TestDivideBits( MaxValue,
                      false,
                      TestIndex,
                      ToDivide,
                      DivideBy,
                      Quotient,
                      Remainder );

      if( TestIndex == 0 )
        break;

      TestIndex--;
      }

    Test1.Copy( Quotient );
    Test1.Multiply( DivideBy );
    Remainder.Copy( ToDivide );
    Remainder.Subtract( Test1 );
    //////////////////////////////
    if( DivideBy.ParamIsGreater( Remainder ))
      throw( new Exception( "LgRemainder > LgDivideBy in LongDivide2()." ));

    ////////////////////////////////
    if( Remainder.IsZero() )
      return true;
    else
      return false;

    }
  */



    // If you multiply the numerator and the denominator by the same amount
    // then the quotient is still the same.  By shifting left (multiplying by twos)
    // the MaxValue upper limit is more accurate.
    // This is called normalization.
  internal int FindShiftBy( ulong ToTest )
    {
    int ShiftBy = 0;
    // If it's not already shifted all the way over to the left,
    // shift it all the way over.
    for( int Count = 0; Count < 32; Count++ )
      {
      if( (ToTest & 0x80000000) != 0 )
        break;

      ShiftBy++;
      ToTest <<= 1;
      }

    return ShiftBy;
    }



  private void LongDivide3( Integer ToDivide,
                            Integer DivideBy,
                            Integer Quotient,
                            Integer Remainder )
    {
    //////////////////
    Integer Test2 = new Integer();
    /////////////////

    int TestIndex = ToDivide.GetIndex() - DivideBy.GetIndex();
    if( TestIndex < 0 )
      throw( new Exception( "TestIndex < 0 in Divide3." ));

    if( TestIndex != 0 )
      {
      // Is 1 too high?
      TestForDivide1.SetDigitAndClear( TestIndex, 1 );
      IntMath.MultiplyTopOne( TestForDivide1, DivideBy );
      if( ToDivide.ParamIsGreater( TestForDivide1 ))
        TestIndex--;

      }

    // Keep a copy of the originals.
    ToDivideKeep.Copy( ToDivide );
    DivideByKeep.Copy( DivideBy );
    ulong TestBits = DivideBy.GetD( DivideBy.GetIndex());
    int ShiftBy = FindShiftBy( TestBits );
    ToDivide.ShiftLeft( ShiftBy ); // Multiply the numerator and the denominator
    DivideBy.ShiftLeft( ShiftBy ); // by the same amount.
    ulong MaxValue;
    if( (ToDivide.GetIndex() - 1) > (DivideBy.GetIndex() + TestIndex) )
      {
      MaxValue = ToDivide.GetD( ToDivide.GetIndex());
      }
    else
      {
      MaxValue = ToDivide.GetD( ToDivide.GetIndex()) << 32;
      MaxValue |= ToDivide.GetD( ToDivide.GetIndex() - 1 );
      }

    ulong Denom = DivideBy.GetD( DivideBy.GetIndex());
    if( Denom != 0 )
      MaxValue = MaxValue / Denom;
    else
      MaxValue = 0xFFFFFFFF;

    if( MaxValue > 0xFFFFFFFF )
      MaxValue = 0xFFFFFFFF;

    if( MaxValue == 0 )
      throw( new Exception( "MaxValue is zero at the top in LongDivide3()." ));

    Quotient.SetDigitAndClear( TestIndex, 1 );
    Quotient.SetD( TestIndex, 0 );
    TestForDivide1.Copy( Quotient );
    TestForDivide1.SetD( TestIndex, MaxValue );
    IntMath.MultiplyTop( TestForDivide1, DivideBy );


/////////////
    Test2.Copy( Quotient );
    Test2.SetD( TestIndex, MaxValue );
    IntMath.Multiply( Test2, DivideBy );
    if( !Test2.IsEqual( TestForDivide1 ))
      throw( new Exception( "In Divide3() !IsEqual( Test2, TestForDivide1 )" ));
///////////



    if( TestForDivide1.ParamIsGreaterOrEq( ToDivide ))
      {
      // ToMatchExactCount++;
      // Most of the time (roughly 5 out of every 6 times)
      // this MaxValue estimate is exactly right:
      Quotient.SetD( TestIndex, MaxValue );
      }
    else
      {
      // MaxValue can't be zero here. If it was it would
      // already be low enough before it got here.
      MaxValue--;
      if( MaxValue == 0 )
        throw( new Exception( "After decrement: MaxValue is zero in LongDivide3()." ));

      TestForDivide1.Copy( Quotient );
      TestForDivide1.SetD( TestIndex, MaxValue );
      IntMath.MultiplyTop( TestForDivide1, DivideBy );
      if( TestForDivide1.ParamIsGreaterOrEq( ToDivide ))
        {
        // ToMatchDecCount++;
        Quotient.SetD( TestIndex, MaxValue );
        }
      else
        {
        // TestDivideBits is done as a last resort, but it's rare.
        // But it does at least limit it to a worst case scenario
        // of trying 32 bits, rather than 4 billion or so decrements.
        TestDivideBits( MaxValue,
                        true,
                        TestIndex,
                        ToDivide,
                        DivideBy,
                        Quotient,
                        Remainder );
        }

      // TestGap = MaxValue - LgQuotient.D[TestIndex];
      // if( TestGap > HighestToMatchGap )
        // HighestToMatchGap = TestGap;

      // HighestToMatchGap: 4,294,967,293
      // uint size:         4,294,967,295 uint
      }

    // If it's done.
    if( TestIndex == 0 )
      {
      TestForDivide1.Copy( Quotient );
      IntMath.Multiply( TestForDivide1, DivideByKeep );
      Remainder.Copy( ToDivideKeep );
      IntMath.Subtract( Remainder, TestForDivide1 );
      //if( DivideByKeep.ParamIsGreater( Remainder ))
        // throw( new Exception( "Remainder > DivideBy in LongDivide3()." ));

      return;
      }

    // Now do the rest of the digits.
    TestIndex--;
    while( true )
      {
      TestForDivide1.Copy( Quotient );
      // First Multiply() for each digit.
      IntMath.Multiply( TestForDivide1, DivideBy );
      // if( ToDivide.ParamIsGreater( TestForDivide1 ))
      //   throw( new Exception( "Problem here in LongDivide3()." ));
      Remainder.Copy( ToDivide );
      IntMath.Subtract( Remainder, TestForDivide1 );
      MaxValue = Remainder.GetD( Remainder.GetIndex()) << 32;
      int CheckIndex = Remainder.GetIndex() - 1;
      if( CheckIndex > 0 )
        MaxValue |= Remainder.GetD( CheckIndex );

      Denom = DivideBy.GetD( DivideBy.GetIndex());
      if( Denom != 0 )
        MaxValue = MaxValue / Denom;
      else
        MaxValue = 0xFFFFFFFF;

      if( MaxValue > 0xFFFFFFFF )
        MaxValue = 0xFFFFFFFF;

      TestForDivide1.Copy( Quotient );
      TestForDivide1.SetD( TestIndex, MaxValue );
      // There's a minimum of two full Multiply() operations per digit.
      IntMath.Multiply( TestForDivide1, DivideBy );
      if( TestForDivide1.ParamIsGreaterOrEq( ToDivide ))
        {
        // Most of the time this MaxValue estimate is exactly right:
        // ToMatchExactCount++;
        Quotient.SetD( TestIndex, MaxValue );
        }
      else
        {
        MaxValue--;
        TestForDivide1.Copy( Quotient );
        TestForDivide1.SetD( TestIndex, MaxValue );
        IntMath.Multiply( TestForDivide1, DivideBy );
        if( TestForDivide1.ParamIsGreaterOrEq( ToDivide ))
          {
          // ToMatchDecCount++;
          Quotient.SetD( TestIndex, MaxValue );
          }
        else
          {
          TestDivideBits( MaxValue,
                          false,
                          TestIndex,
                          ToDivide,
                          DivideBy,
                          Quotient,
                          Remainder );

          // TestGap = MaxValue - LgQuotient.D[TestIndex];
          // if( TestGap > HighestToMatchGap )
            // HighestToMatchGap = TestGap;
          }
        }

      if( TestIndex == 0 )
        break;

      TestIndex--;
      }

    TestForDivide1.Copy( Quotient );
    IntMath.Multiply( TestForDivide1, DivideByKeep );
    Remainder.Copy( ToDivideKeep );
    IntMath.Subtract( Remainder, TestForDivide1 );
    // if( DivideByKeep.ParamIsGreater( Remainder ))
      // throw( new Exception( "Remainder > DivideBy in LongDivide3()." ));
    }




  }
}


