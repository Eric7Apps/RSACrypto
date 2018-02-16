// Copyright Eric Chauvin 2015 - 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Text;
using System.ComponentModel; // BackgroundWorker


namespace RSACrypto
{

  class ChineseRemainder
  {
  private int[] DigitsArray;
  private IntegerMath IntMath;
  // This has to be set in relation to the Integer.DigitArraySize so that
  // it isn't too big for the MultplyUint that's done in
  // GetTraditionalInteger().  Also it has to be checked with the Max
  // Value test.
  internal const int DigitsArraySize = Integer.DigitArraySize * 2;



  private ChineseRemainder()
    {
    }



  internal ChineseRemainder( IntegerMath UseIntMath )
    {
    if( DigitsArraySize > IntegerMath.PrimeArrayLength )
      throw( new Exception( "ChineseRemainder digit size is too big." ));

    IntMath = UseIntMath;

    DigitsArray = new int[DigitsArraySize];
    // SetToZero(); Not necessary for managed code.
    }



  internal int GetDigitAt( int Index )
    {
    if( Index >= DigitsArraySize )
      throw( new Exception( "ChineseRemainder GetDigitAt Index is too big." ));

    return DigitsArray[Index];
    }



  internal void SetDigitAt( int SetTo, int Index )
    {
    if( Index >= DigitsArraySize )
      throw( new Exception( "ChineseRemainder SetDigitAt Index is too big." ));

    DigitsArray[Index] = SetTo;
    }



  internal void SetToZero()
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      DigitsArray[Count] = 0;

    }



  internal bool IsZero()
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      if( DigitsArray[Count] != 0 )
        return false;

      }

    return true;
    }



  internal void SetToOne()
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      DigitsArray[Count] = 1;

    }


  internal bool IsOne()
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      if( DigitsArray[Count] != 1 )
        return false;

      }

    return true;
    }



  internal void Copy( ChineseRemainder ToCopy )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] = ToCopy.DigitsArray[Count];
      }
    }



  internal bool IsEqual( ChineseRemainder ToCheck )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      if( DigitsArray[Count] != ToCheck.DigitsArray[Count] )
        return false;

      }

    return true;
    }



  internal void Add( ChineseRemainder ToAdd )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      // Operations like this could be very fast if
      // they were done on a GPU processor or with
      // Intel's Advanced Vector Extensions.
      // They could be done in parallel, which would
      // make it a lot faster than the way this is
      // done, one digit at a time.  Notice that
      // there is no carry operation here.  As Claud
      // Shannon would say, there is no diffusion here.
      DigitsArray[Count] += ToAdd.DigitsArray[Count];
      int Prime = (int)IntMath.GetPrimeAt( Count );
      if( DigitsArray[Count] >= Prime )
        DigitsArray[Count] -= Prime;
        // DigitsArray[Count] = DigitsArray[Count] % Prime;

      }
    }



  internal void Subtract( ChineseRemainder ToSub )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] -= ToSub.DigitsArray[Count];
      if( DigitsArray[Count] < 0 )
        DigitsArray[Count] += (int)IntMath.GetPrimeAt( Count );

      }
    }



  internal void Decrement1()
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] -= 1;
      if( DigitsArray[Count] < 0 )
        DigitsArray[Count] += (int)IntMath.GetPrimeAt( Count );

      }
    }


  internal void SubtractUInt( uint ToSub )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] -= (int)(ToSub % (int)IntMath.GetPrimeAt( Count ));
      if( DigitsArray[Count] < 0 )
        DigitsArray[Count] += (int)IntMath.GetPrimeAt( Count );

      }
    }


  // Copyright Eric Chauvin.
  internal void Multiply( ChineseRemainder ToMul )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      // There is no Diffusion here either, like the
      // kind that Claude Shannon wrote about in
      // A Mathematical Theory of Cryptography.
      DigitsArray[Count] *= ToMul.DigitsArray[Count];
      DigitsArray[Count] %= (int)IntMath.GetPrimeAt( Count );
      }
    }



  internal void SetFromTraditionalInteger( Integer SetFrom )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] = (int)IntMath.Divider.GetMod32( SetFrom, IntMath.GetPrimeAt( Count ));
      }
    }



  internal void SetFromUInt( uint SetFrom )
    {
    for( int Count = 0; Count < DigitsArraySize; Count++ )
      {
      DigitsArray[Count] = (int)(SetFrom % (int)IntMath.GetPrimeAt( Count ));
      }
    }



  internal string GetString()
    {
    StringBuilder SBuilder = new StringBuilder();
    for( int Count = 20; Count >= 0; Count-- )
      {
      string ShowS = DigitsArray[Count].ToString() + ", ";
      // DigitsArray[Count].Prime

      SBuilder.Append( ShowS );
      }

    return SBuilder.ToString();
    }


  }
}




