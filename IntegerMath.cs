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


using System;
using System.Text;
using System.ComponentModel; // BackgroundWorker


namespace RSACrypto
{
  class IntegerMath
  {
  private MainForm MForm;
  internal Division Divider;
  private long[] SignedD; // Signed digits for use in subtraction.
  private ulong[,] M; // Scratch pad, just like you would do on paper.
  private ulong[] Scratch; // Scratch pad, just like you would do on paper.
  // I don't want to create any of these numbers
  // inside a loop so they are created just once here.
  private Integer ToDivide = new Integer();
  private Integer Quotient = new Integer();
  private Integer Remainder = new Integer();
  private Integer TempAdd1 = new Integer();
  private Integer TempAdd2 = new Integer();
  private Integer TempSub1 = new Integer();
  private Integer TempSub2 = new Integer();
  private Integer GcdX = new Integer();
  private Integer GcdY = new Integer();
  private Integer TestForModInverse1 = new Integer();
  private Integer TestForModInverse2 = new Integer();
  private Integer U0 = new Integer();
  private Integer U1 = new Integer();
  private Integer U2 = new Integer();
  private Integer V0 = new Integer();
  private Integer V1 = new Integer();
  private Integer V2 = new Integer();
  private Integer T0 = new Integer();
  private Integer T1 = new Integer();
  private Integer T2 = new Integer();
  private Integer SubtractTemp1 = new Integer();
  private Integer SubtractTemp2 = new Integer();
  private Integer Fermat1 = new Integer();
  private Integer Fermat2 = new Integer();
  private Integer TestFermat = new Integer();
  private Integer TempEuclid1 = new Integer();
  private Integer TempEuclid2 = new Integer();
  private Integer TempEuclid3 = new Integer();
  private Integer TestForBits = new Integer();
  private Integer TestForSquareRoot = new Integer();
  private Integer SqrtXPartTest1 = new Integer();
  private Integer SqrtXPartTest2 = new Integer();
  private Integer SqrtXPartDiff = new Integer();
  private Integer SqrtXPartR2 = new Integer();
  private Integer SqrtXPartTwoB = new Integer();
  private uint[] PrimeArray;
  internal const int PrimeArrayLength = 1024 * 16;
  private bool Cancelled = false;
  private Integer[] GeneralBaseArray;
  private Integer CurrentModReductionBase = new Integer();
  private Integer TempForModPower = new Integer();
  private Integer TestForModPower = new Integer();
  private Integer TestForModReduction2 = new Integer();
  private Integer TestForModReduction2ForModPower = new Integer();
  private Integer XForModPower = new Integer();
  private Integer ExponentCopy = new Integer();
  private Integer AccumulateBase = new Integer();
  private int MaxModPowerIndex = 0;



  private IntegerMath()
    {
    }


  internal IntegerMath( MainForm UseForm )
    {
    MForm = UseForm;
    Divider = new Division( MForm, this );
    SignedD = new long[Integer.DigitArraySize];
    M = new ulong[Integer.DigitArraySize, Integer.DigitArraySize];
    Scratch = new ulong[Integer.DigitArraySize];

    MakePrimeArray();
    }



  private void ShowStatus( string ToShow )
    {
    if( MForm == null )
      return;

    MForm.ShowStatus( ToShow );
    }



  internal void SetCancelled( bool SetTo )
    {
    Cancelled = SetTo;
    }



  internal uint GetBiggestPrime()
    {
    return PrimeArray[PrimeArrayLength - 1];
    }



  internal uint GetPrimeAt( int Index )
    {
    if( Index >= PrimeArrayLength )
      return 0;

    return PrimeArray[Index];
    }



  internal uint GetFirstPrimeFactor( ulong ToTest )
    {
    if( ToTest < 2 )
      return 0;

    if( ToTest == 2 )
      return 2;

    if( ToTest == 3 )
      return 3;

    uint Max = (uint)FindULSqrRoot( ToTest );
    for( int Count = 0; Count < PrimeArrayLength; Count++ )
      {
      uint TestN = PrimeArray[Count];
      if( (ToTest % TestN) == 0 )
        return TestN;

      if( TestN > Max )
        return 0;

      }

    return 0;
    }



  private void MakePrimeArray()
    {
    // try
    PrimeArray = new uint[PrimeArrayLength];
    // catch
    PrimeArray[0] = 2;
    PrimeArray[1] = 3;
    PrimeArray[2] = 5;
    PrimeArray[3] = 7;
    PrimeArray[4] = 11;
    PrimeArray[5] = 13;
    PrimeArray[6] = 17;
    PrimeArray[7] = 19;
    PrimeArray[8] = 23;

    int Last = 9;
    for( uint TestN = 29; ; TestN += 2 )
      {
      if( (TestN % 3) == 0 )
        continue;

      // If it has no prime factors then add it.
      if( 0 == GetFirstPrimeFactor( TestN ))
        {
        PrimeArray[Last] = TestN;
        // if( (Last + 100) > PrimeArray.Length )
        // if( Last < 160 )
          // StatusString += Last.ToString() + ") " + PrimeArray[Last].ToString() + ", ";

        Last++;
        if( Last >= PrimeArrayLength )
          return;

        }
      }
    }



  internal uint IsDivisibleBySmallPrime( Integer ToTest )
    {
    if( (ToTest.GetD( 0 ) & 1) == 0 )
      return 2; // It's divisible by 2.

    for( int Count = 1; Count < PrimeArrayLength; Count++ )
      {
      if( 0 == Divider.GetMod32( ToTest, PrimeArray[Count] ))
        return PrimeArray[Count];

      }

    // No small primes divide it.
    return 0;
    }



  internal ulong GetSumOfPrimesUpToAndIncluding( int UpTo )
    {
    if( UpTo >= PrimeArrayLength )
      throw( new Exception( "Can't go past the array for GetSumOfPrimesUpTo()." ));

    ulong Sum = 0;
    for( int Count = 0; Count < UpTo; Count++ )
      Sum += PrimeArray[Count];

    return Sum;
    }



  internal void SubtractULong( Integer Result, ulong ToSub )
    {
    if( Result.IsULong())
      {
      ulong ResultU = Result.GetAsULong();
      if( ToSub > ResultU )
        throw( new Exception( "SubULong() (IsULong() and (ToSub > Result)." ));

      ResultU = ResultU - ToSub;
      Result.SetD( 0, ResultU & 0xFFFFFFFF );
      Result.SetD( 1, ResultU >> 32 );
      if( Result.GetD( 1 ) == 0 )
        Result.SetIndex( 0 );
      else
        Result.SetIndex( 1 );

      return;
      }

    // If it got this far then Index is at least 2.
    SignedD[0] = (long)Result.GetD( 0 ) - (long)(ToSub & 0xFFFFFFFF);
    SignedD[1] = (long)Result.GetD( 1 ) - (long)(ToSub >> 32);
    if( (SignedD[0] >= 0) && (SignedD[1] >= 0) )
      {
      // No need to reorganize it.
      Result.SetD( 0, (ulong)SignedD[0] );
      Result.SetD( 1, (ulong)SignedD[1] );
      return;
      }

    for( int Count = 2; Count <= Result.GetIndex(); Count++ )
      SignedD[Count] = (long)Result.GetD( Count );

    for( int Count = 0; Count < Result.GetIndex(); Count++ )
      {
      if( SignedD[Count] < 0 )
        {
        SignedD[Count] += (long)0xFFFFFFFF + 1;
        SignedD[Count + 1]--;
        }
      }

    if( SignedD[Result.GetIndex()] < 0 )
      throw( new Exception( "SubULong() SignedD[Index] < 0." ));

    for( int Count = 0; Count <= Result.GetIndex(); Count++ )
      Result.SetD( Count, (ulong)SignedD[Count] );

    for( int Count = Result.GetIndex(); Count >= 0; Count-- )
      {
      if( Result.GetD( Count ) != 0 )
        {
        Result.SetIndex( Count );
        return;
        }
      }

    // If this was zero it wouldn't find a nonzero
    // digit to set the Index to and it would end up down here.
    Result.SetIndex( 0 );
    }



  internal void Add( Integer Result, Integer ToAdd )
    {
    if( ToAdd.IsZero())
      return;

    // The most common form.  They are both positive.
    if( !Result.IsNegative && !ToAdd.IsNegative )
      {
      Result.Add( ToAdd );
      return;
      }

    if( !Result.IsNegative && ToAdd.IsNegative )
      {
      TempAdd1.Copy( ToAdd );
      TempAdd1.IsNegative = false;
      if( TempAdd1.ParamIsGreater( Result ))
        {
        Subtract( Result, TempAdd1 );
        return;
        }
      else
        {
        Subtract( TempAdd1, Result );
        Result.Copy( TempAdd1 );
        Result.IsNegative = true;
        return;
        }
      }

    if( Result.IsNegative && !ToAdd.IsNegative )
      {
      TempAdd1.Copy( Result );
      TempAdd1.IsNegative = false;
      TempAdd2.Copy( ToAdd );
      if( TempAdd1.ParamIsGreater( TempAdd2 ))
        {
        Subtract( TempAdd2, TempAdd1 );
        Result.Copy( TempAdd2 );
        return;
        }
      else
        {
        Subtract( TempAdd1, TempAdd2 );
        Result.Copy( TempAdd2 );
        Result.IsNegative = true;
        return;
        }
      }

    if( Result.IsNegative && ToAdd.IsNegative )
      {
      TempAdd1.Copy( Result );
      TempAdd1.IsNegative = false;
      TempAdd2.Copy( ToAdd );
      TempAdd2.IsNegative = false;
      TempAdd1.Add( TempAdd2 );
      Result.Copy( TempAdd1 );
      Result.IsNegative = true;
      return;
      }
    }



  internal void Subtract( Integer Result, Integer ToSub )
    {
    // This checks that the sign is equal too.
    if( Result.IsEqual( ToSub ))
      {
      Result.SetToZero();
      return;
      }

    // ParamIsGreater() handles positive and negative values, so if the
    // parameter is more toward the positive side then it's true.  It's greater.
    // The most common form.  They are both positive.
    if( !Result.IsNegative && !ToSub.IsNegative )
      {
      if( ToSub.ParamIsGreater( Result ))
        {
        SubtractPositive( Result, ToSub );
        return;
        }

      // ToSub is bigger.
      TempSub1.Copy( Result );
      TempSub2.Copy( ToSub );
      SubtractPositive( TempSub2, TempSub1 );
      Result.Copy( TempSub2 );
      Result.IsNegative = true;
      return;
      }

    if( Result.IsNegative && !ToSub.IsNegative )
      {
      TempSub1.Copy( Result );
      TempSub1.IsNegative = false;
      TempSub1.Add( ToSub );
      Result.Copy( TempSub1 );
      Result.IsNegative = true;
      return;
      }

    if( !Result.IsNegative && ToSub.IsNegative )
      {
      TempSub1.Copy( ToSub );
      TempSub1.IsNegative = false;
      Result.Add( TempSub1 );
      return;
      }

    if( Result.IsNegative && ToSub.IsNegative )
      {
      TempSub1.Copy( Result );
      TempSub1.IsNegative = false;
      TempSub2.Copy( ToSub );
      TempSub2.IsNegative = false;
      // -12 - -7 = -12 + 7 = -5
      // Comparing the positive numbers here.
      if( TempSub2.ParamIsGreater( TempSub1 ))
        {
        SubtractPositive( TempSub1, TempSub2 );
        Result.Copy( TempSub1 );
        Result.IsNegative = true;
        return;
        }

      // -7 - -12 = -7 + 12 = 5
      SubtractPositive( TempSub2, TempSub1 );
      Result.Copy( TempSub2 );
      Result.IsNegative = false;
      return;
      }
    }



  internal void SubtractPositive( Integer Result, Integer ToSub )
    {
    if( ToSub.IsULong() )
      {
      SubtractULong( Result, ToSub.GetAsULong());
      return;
      }

    if( ToSub.GetIndex() > Result.GetIndex() )
      throw( new Exception( "In Subtract() ToSub.Index > Index." ));

    for( int Count = 0; Count <= ToSub.GetIndex(); Count++ )
      SignedD[Count] = (long)Result.GetD( Count ) - (long)ToSub.GetD( Count );

    for( int Count = ToSub.GetIndex() + 1; Count <= Result.GetIndex(); Count++ )
      SignedD[Count] = (long)Result.GetD( Count );

    for( int Count = 0; Count < Result.GetIndex(); Count++ )
      {
      if( SignedD[Count] < 0 )
        {
        SignedD[Count] += (long)0xFFFFFFFF + 1;
        SignedD[Count + 1]--;
        }
      }

    if( SignedD[Result.GetIndex()] < 0 )
      throw( new Exception( "Subtract() SignedD[Index] < 0." ));

    for( int Count = 0; Count <= Result.GetIndex(); Count++ )
      Result.SetD( Count, (ulong)SignedD[Count] );

    for( int Count = Result.GetIndex(); Count >= 0; Count-- )
      {
      if( Result.GetD( Count ) != 0 )
        {
        Result.SetIndex( Count );
        return;
        }
      }

    // If it never found a non-zero digit it would get down to here.
    Result.SetIndex( 0 );
    }



  internal void MultiplyUInt( Integer Result, ulong ToMul )
    {
    try
    {
    if( ToMul == 0 )
      {
      Result.SetToZero();
      return;
      }

    if( ToMul == 1 )
      return;

    int CountTo = Result.GetIndex();
    for( int Column = 0; Column <= CountTo; Column++ )
      M[Column, 0] = ToMul * Result.GetD( Column );

    // Add these up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    CountTo = Result.GetIndex();
    for( int Column = 1; Column <= CountTo; Column++ )
      {
      // Using a compile-time check on this constant,
      // this Test value does not overflow:
      // const ulong Test = ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF)) + 0xFFFFFFFF;
      // ulong Total = checked( M[Column, 0] + Carry );
      ulong Total = M[Column, 0] + Carry;
      Result.SetD( Column, Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This might throw an exception if it overflows.
      Result.SetD( Result.GetIndex(), Carry );
      }
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in MultiplyUInt(): " + Except.Message ));
      }
    }



  internal int MultiplyUIntFromCopy( Integer Result, Integer FromCopy, ulong ToMul )
    {
    int FromCopyIndex = FromCopy.GetIndex();
    Result.SetIndex( FromCopyIndex );
    for( int Column = 0; Column <= FromCopyIndex; Column++ )
      Scratch[Column] = ToMul * FromCopy.GetD( Column );

    // Add these up with a carry.
    Result.SetD( 0, Scratch[0] & 0xFFFFFFFF );
    ulong Carry = Scratch[0] >> 32;
    for( int Column = 1; Column <= FromCopyIndex; Column++ )
      {
      ulong Total = Scratch[Column] + Carry;
      Result.SetD( Column, Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This might throw an exception if it overflows.
      Result.SetD( FromCopyIndex + 1, Carry );
      }

    return Result.GetIndex();
    }



  internal void MultiplyULong( Integer Result, ulong ToMul )
    {
    // Using compile-time checks, this one overflows:
    // const ulong Test = ((ulong)0xFFFFFFFF + 1) * ((ulong)0xFFFFFFFF + 1);
    // This one doesn't:
    // const ulong Test = (ulong)0xFFFFFFFF * ((ulong)0xFFFFFFFF + 1);
    if( Result.IsZero())
      return; // Then the answer is zero, which it already is.

    if( ToMul == 0 )
      {
      Result.SetToZero();
      return;
      }

    ulong B0 = ToMul & 0xFFFFFFFF;
    ulong B1 = ToMul >> 32;
    if( B1 == 0 )
      {
      MultiplyUInt( Result, (uint)B0 );
      return;
      }

    // Since B1 is not zero:
    if( (Result.GetIndex() + 1) >= Integer.DigitArraySize )
      throw( new Exception( "Overflow in MultiplyULong." ));

    int CountTo = Result.GetIndex();
    for( int Column = 0; Column <= CountTo; Column++ )
      {
      M[Column, 0] = B0 * Result.GetD( Column );
      // Column + 1 and Row is 1, so it's just like pen and paper.
      M[Column + 1, 1] = B1 * Result.GetD( Column );
      }

    // Since B1 is not zero, the index is set one higher.
    Result.IncrementIndex(); // Might throw an exception if it goes out of range.
    M[Result.GetIndex(), 0] = 0; // Otherwise it would be undefined
                                 // when it's added up below.
    // Add these up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    CountTo = Result.GetIndex();
    for( int Column = 1; Column <= CountTo; Column++ )
      {
      // This does overflow:
      // const ulong Test = ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF))
      //                  + ((ulong)0xFFFFFFFF * (ulong)(0xFFFFFFFF));
      // Split the ulongs into right and left sides
      // so that they don't overflow.
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      // There's only the two rows for this.
      for( int Row = 0; Row <= 1; Row++ )
        {
        TotalRight += M[Column, Row] & 0xFFFFFFFF;
        TotalLeft += M[Column, Row] >> 32;
        }

      TotalRight += Carry;
      Result.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This can throw an exception.
      Result.SetD( Result.GetIndex(), Carry );
      }
    }



  private void SetMultiplySign( Integer Result, Integer ToMul )
    {
    if( Result.IsNegative == ToMul.IsNegative )
      Result.IsNegative = false;
    else
      Result.IsNegative = true;
    }



  // See also: http://en.wikipedia.org/wiki/Karatsuba_algorithm
  internal void Multiply( Integer Result, Integer ToMul )
    {
    // try
    // {
    if( Result.IsZero())
      return;

    if( ToMul.IsULong())
      {
      MultiplyULong( Result, ToMul.GetAsULong());
      SetMultiplySign( Result, ToMul );
      return;
      }

    // It could never get here if ToMul is zero because GetIsULong()
    // would be true for zero.
    // if( ToMul.IsZero())
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "Multiply() overflow." ));

    int CountTo = ToMul.GetIndex();
    for( int Row = 0; Row <= CountTo; Row++ )
      {
      if( ToMul.GetD( Row ) == 0 )
        {
        int CountZeros = Result.GetIndex();
        for( int Column = 0; Column <= CountZeros; Column++ )
          M[Column + Row, Row] = 0;

        }
      else
        {
        int CountMult = Result.GetIndex();
        for( int Column = 0; Column <= CountMult; Column++ )
          M[Column + Row, Row] = ToMul.GetD( Row ) * Result.GetD( Column );

        }
      }

    // Add the columns up with a carry.
    Result.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    int ResultIndex = Result.GetIndex();
    int MulIndex = ToMul.GetIndex();
    for( int Column = 1; Column <= TotalIndex; Column++ )
      {
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      for( int Row = 0; Row <= MulIndex; Row++ )
        {
        if( Row > Column )
          break;

        if( Column > (ResultIndex + Row) )
          continue;

        // Split the ulongs into right and left sides
        // so that they don't overflow.
        TotalRight += M[Column, Row] & 0xFFFFFFFF;
        TotalLeft += M[Column, Row] >> 32;
        }

      TotalRight += Carry;
      Result.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    Result.SetIndex( TotalIndex );
    if( Carry != 0 )
      {
      Result.IncrementIndex(); // This can throw an exception if it overflowed the index.
      Result.SetD( Result.GetIndex(), Carry );
      }

    SetMultiplySign( Result, ToMul );
    }




  internal void SetFromString( Integer Result, string InString )
    {
    if( InString == null )
      throw( new Exception( "InString was null in SetFromString()." ));

    if( InString.Length < 1 )
      {
      Result.SetToZero();
      return;
      }

    Base10Number Base10N = new Base10Number();
    Integer Tens = new Integer();
    Integer OnePart = new Integer();
    // This might throw an exception if the string is bad.
    Base10N.SetFromString( InString );
    Result.SetFromULong( Base10N.GetD( 0 ));
    Tens.SetFromULong( 10 );
    for( int Count = 1; Count <= Base10N.GetIndex(); Count++ )
      {
      OnePart.SetFromULong( Base10N.GetD( Count ));
      Multiply( OnePart, Tens );
      Result.Add( OnePart );
      MultiplyULong( Tens, 10 );
      }
    }



  internal string ToString10( Integer From )
    {
    if( From.IsULong())
      {
      ulong N = From.GetAsULong();
      if( From.IsNegative )
        return "-" + N.ToString( "N0" );
      else
        return N.ToString( "N0" );

      }

    string Result = "";
    ToDivide.Copy( From );
    int CommaCount = 0;
    while( !ToDivide.IsZero())
      {
      uint Digit = (uint)Divider.ShortDivideRem( ToDivide, 10, Quotient );
      ToDivide.Copy( Quotient );
      if( ((CommaCount % 3) == 0) && (CommaCount != 0) )
        Result = Digit.ToString() + "," + Result; // Or use a StringBuilder.
      else
        Result = Digit.ToString() + Result;

      CommaCount++;
      }

    if( From.IsNegative )
      return "-" + Result;
    else
      return Result;

    }



  internal static bool IsSmallQuadResidue( uint Number )
    {
    // For mod 2:
    // 1 * 1 = 1 % 2 = 1
    // 0 * 0 = 0 % 2 = 0
    uint Test = Number % 3; // 0, 1, 1, 0
    if( Test == 2 )
      return false;

    Test = Number % 5;
    if( (Test == 2) || (Test == 3))  // 0, 1, 4, 4, 1, 0
      return false;

    Test = Number % 7;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 2)) )
      return false;

    Test = Number % 11;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 5) ||
          (Test == 3)) )
      return false;

    Test = Number % 13;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 3) ||
          (Test == 12) ||
          (Test == 10)) )
      return false;

    // If it made it this far...
    return true;
    }



  internal static bool IsQuadResidue17To23( uint Number )
    {
    uint Test = Number % 17;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 8) ||
          (Test == 2) ||
          (Test == 15) ||
          (Test == 13)) )
      return false;

    Test = Number % 19;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 6) ||
          (Test == 17) ||
          (Test == 11) ||
          (Test == 7) ||
          (Test == 5)) )
      return false;

    Test = Number % 23;
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 2) ||
          (Test == 13) ||
          (Test == 3) ||
          (Test == 18) ||
          (Test == 12) ||
          (Test == 8) ||
          (Test == 6)) )
      return false;

    // If it made it this far...
    return true;
    }



  internal static bool IsQuadResidue29( ulong Number )
    {
    uint Test = (uint)(Number % 29);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 7) ||
          (Test == 20) ||
          (Test == 6) ||
          (Test == 23) ||
          (Test == 13) ||
          (Test == 5) ||
          (Test == 28) ||
          (Test == 24) ||
          (Test == 22)) )
      return false;

    return true;
    }



  internal static bool IsQuadResidue31( ulong Number )
    {
    uint Test = (uint)(Number % 31);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 5) ||
          (Test == 18) ||
          (Test == 2) ||
          (Test == 19) ||
          (Test == 7) ||
          (Test == 28) ||
          (Test == 20) ||
          (Test == 14) ||
          (Test == 10) ||
          (Test == 8)))
      return false;

    return true;
    }



  internal static bool IsQuadResidue37( ulong Number )
    {
    uint Test = (uint)(Number % 37);
    if( !((Test == 0) ||
          (Test == 1) ||
          (Test == 4) ||
          (Test == 9) ||
          (Test == 16) ||
          (Test == 25) ||
          (Test == 36) ||
          (Test == 12) ||
          (Test == 27) ||
          (Test == 7) ||
          (Test == 26) ||
          (Test == 10) ||
          (Test == 33) ||
          (Test == 21) ||
          (Test == 11) ||
          (Test == 3) ||
          (Test == 34) ||
          (Test == 30) ||
          (Test == 28)))
      return false;

    return true;
    }



  internal static bool FirstBytesAreQuadRes( uint Test )
    {
    // Is this number a square mod 2^12?
    // (Quadratic residue mod 2^12)
    uint FirstByte = Test;
    uint SecondByte = (FirstByte & 0x0F00) >> 8;
    // The bottom 4 bits can only be 0, 1, 4 or 9
    // 0000, 0001, 0100 or 1001
    // The bottom 2 bits can only be 00 or 01
    FirstByte = FirstByte & 0x0FF;
    switch( FirstByte )
      {
      case 0x00: // return true;
        if( (SecondByte == 0) ||
            (SecondByte == 1) ||
            (SecondByte == 4) ||
            (SecondByte == 9))
          return true;
        else
          return false;

      case 0x01: return true;
      case 0x04: return true;
      case 0x09: return true;
      case 0x10: return true;
      case 0x11: return true;
      case 0x19: return true;
      case 0x21: return true;
      case 0x24: return true;
      case 0x29: return true;
      case 0x31: return true;
      case 0x39: return true;
      case 0x40: // return true;
        // 0x40, 0, 2, 4, 6, 8, 10, 12, 14
        if( (SecondByte & 0x01) == 0x01 )
          return false;
        else
          return true;

      case 0x41: return true;
      case 0x44: return true;
      case 0x49: return true;
      case 0x51: return true;
      case 0x59: return true;
      case 0x61: return true;
      case 0x64: return true;
      case 0x69: return true;
      case 0x71: return true;
      case 0x79: return true;
      case 0x81: return true;
      case 0x84: return true;
      case 0x89: return true;
      case 0x90: return true;
      case 0x91: return true;
      case 0x99: return true;
      case 0xA1: return true;
      case 0xA4: return true;
      case 0xA9: return true;
      case 0xB1: return true;
      case 0xB9: return true;
      case 0xC1: return true;
      case 0xC4: return true;
      case 0xC9: return true;
      case 0xD1: return true;
      case 0xD9: return true;
      case 0xE1: return true;
      case 0xE4: return true;
      case 0xE9: return true;
      case 0xF1: return true;
      case 0xF9: return true;  // 44 out of 256.

      default: return false;
      }
    }



  internal void DoSquare( Integer ToSquare )
    {
    if( ToSquare.GetIndex() == 0 )
      {
      ToSquare.Square0();
      return;
      }

    if( ToSquare.GetIndex() == 1 )
      {
      ToSquare.Square1();
      return;
      }

    if( ToSquare.GetIndex() == 2 )
      {
      ToSquare.Square2();
      return;
      }

    // Now Index is at least 3:
    int DoubleIndex = ToSquare.GetIndex() << 1;
    if( DoubleIndex >= Integer.DigitArraySize )
      {
      throw( new Exception( "Square() overflowed." ));
      }

    for( int Row = 0; Row <= ToSquare.GetIndex(); Row++ )
      {
      if( ToSquare.GetD( Row ) == 0 )
        {
        for( int Column = 0; Column <= ToSquare.GetIndex(); Column++ )
          M[Column + Row, Row] = 0;

        }
      else
        {
        for( int Column = 0; Column <= ToSquare.GetIndex(); Column++ )
          M[Column + Row, Row] = ToSquare.GetD( Row ) * ToSquare.GetD( Column );

        }
      }

    // Add the columns up with a carry.
    ToSquare.SetD( 0, M[0, 0] & 0xFFFFFFFF );
    ulong Carry = M[0, 0] >> 32;
    for( int Column = 1; Column <= DoubleIndex; Column++ )
      {
      ulong TotalLeft = 0;
      ulong TotalRight = 0;
      for( int Row = 0; Row <= Column; Row++ )
        {
        if( Row > ToSquare.GetIndex() )
          break;

        if( Column > (ToSquare.GetIndex() + Row) )
          continue;

        TotalRight += M[Column, Row] & 0xFFFFFFFF;
        TotalLeft += M[Column, Row] >> 32;
        }

      TotalRight += Carry;
      ToSquare.SetD( Column, TotalRight & 0xFFFFFFFF );
      Carry = TotalRight >> 32;
      Carry += TotalLeft;
      }

    ToSquare.SetIndex( DoubleIndex );
    if( Carry != 0 )
      {
      ToSquare.SetIndex( ToSquare.GetIndex() + 1 );
      if( ToSquare.GetIndex() >= Integer.DigitArraySize )
        throw( new Exception( "Square() overflow." ));

      ToSquare.SetD( ToSquare.GetIndex(), Carry );
      }
    }



  internal ulong FindULSqrRoot( ulong ToMatch )
    {
    // Start OneBit with the highest possible bit.
    ulong OneBit = 0x80000000; // 0x8000 0000
    ulong Result = 0;
    for( int Count = 0; Count < 32; Count++ )
      {
      ulong ToTry = Result | OneBit;
      if( (ToTry * ToTry) <= ToMatch )
        Result |= OneBit; // Then I want the bit.

      OneBit >>= 1;
      }

    ////////////////////////////////////////////
    // Test:
    if( (Result * Result) > ToMatch )
      throw( new Exception( "FindULSqrRoot() Result is too high." ));

    // This would overflow if Answer was 0xFFFFFFFF.
    // It won't do overflow checking at run time unless you use
    // the checked keyword.
    if( (Result != 0) && (Result != 0xFFFFFFFF)  )
      {
      if( ((Result + 1) * (Result + 1)) <= ToMatch )
        throw( new Exception( "FindULSqrRoot() Result is too low." ));

      }

    /////////////////////////////////////////
    return Result;
    }



  // This is an optimization for multiplying when
  // only the top digit of a number has been set and
  // all of the other digits are zero.
  internal void MultiplyTop( Integer Result, Integer ToMul )
    {
    // try
    // {
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "MultiplyTop() overflow." ));

    // Just like Multiply() except that all the other rows are zero:
    for( int Column = 0; Column <= ToMul.GetIndex(); Column++ )
      M[Column + Result.GetIndex(), Result.GetIndex()] = Result.GetD( Result.GetIndex() ) * ToMul.GetD( Column );

    for( int Column = 0; Column < Result.GetIndex(); Column++ )
      Result.SetD( Column, 0 );

    ulong Carry = 0;
    for( int Column = 0; Column <= ToMul.GetIndex(); Column++ )
      {
      ulong Total = M[Column + Result.GetIndex(), Result.GetIndex()] + Carry;
      Result.SetD( Column + Result.GetIndex(), Total & 0xFFFFFFFF );
      Carry = Total >> 32;
      }

    Result.SetIndex( TotalIndex );
    if( Carry != 0 )
      {
      Result.SetIndex( Result.GetIndex() + 1 );
      if( Result.GetIndex() >= Integer.DigitArraySize )
        throw( new Exception( "MultiplyTop() overflow." ));

      Result.SetD( Result.GetIndex(), Carry );
      }

    /*
    }
    catch( Exception ) // Except )
      {
      // "Exception in MultiplyTop: " + Except.Message
      }
    */
    }



  // This is another optimization.  This is used when the top digit
  // is 1 and all of the other digits are zero.
  // This is effectively just a shift-left operation.
  internal void MultiplyTopOne( Integer Result, Integer ToMul )
    {
    // try
    // {
    int TotalIndex = Result.GetIndex() + ToMul.GetIndex();
    if( TotalIndex >= Integer.DigitArraySize )
      throw( new Exception( "MultiplyTopOne() overflow." ));

    for( int Column = 0; Column <= ToMul.GetIndex(); Column++ )
      Result.SetD( Column + Result.GetIndex(), ToMul.GetD( Column ));

    for( int Column = 0; Column < Result.GetIndex(); Column++ )
      Result.SetD( Column, 0 );

    // No Carrys need to be done.
    Result.SetIndex( TotalIndex );
    /*
    }
    catch( Exception ) // Except )
      {
      // "Exception in MultiplyTopOne: " + Except.Message
      }
      */
    }



  // Finding the square root of a number is similar to division since
  // it is a search algorithm.  The TestSqrtBits method shown next is
  // very much like TestDivideBits().  It works the same as
  // FindULSqrRoot(), but on a bigger scale.
  /*
  private void TestSqrtBits( int TestIndex, Integer Square, Integer SqrRoot )
    {
    Integer Test1 = new Integer();
    uint BitTest = 0x80000000;
    for( int BitCount = 31; BitCount >= 0; BitCount-- )
      {
      Test1.Copy( SqrRoot );
      Test1.D[TestIndex] |= BitTest;
      Test1.Square();
      if( !Square.ParamIsGreater( Test1 ) )
        SqrRoot.D[TestIndex] |= BitTest; // Use the bit.

      BitTest >>= 1;
      }
    }
    */



  // In the SquareRoot() method SqrRoot.Index is half of Square.Index.
  // Compare this to the Square() method where the Carry might or
  // might not increment the index to an odd number.  (So if the Index
  // was 5 its square root would have an Index of 5 / 2 = 2.)
  // The SquareRoot1() method uses FindULSqrRoot() either to find the
  // whole answer, if it's a small number, or it uses it to find the
  // top part.  Then from there it goes on to a bit by bit search
  // with TestSqrtBits().
  public bool SquareRoot( Integer Square, Integer SqrRoot )
    {
    ulong ToMatch;
    if( Square.IsULong() )
      {
      ToMatch = Square.GetAsULong();
      SqrRoot.SetD( 0, FindULSqrRoot( ToMatch ));
      SqrRoot.SetIndex( 0 );
      if( (SqrRoot.GetD(0 ) * SqrRoot.GetD( 0 )) == ToMatch )
        return true;
      else
        return false;
      }

    int TestIndex = Square.GetIndex() >> 1; // LgSquare.Index / 2;
    SqrRoot.SetDigitAndClear( TestIndex, 1 );
    // if( (TestIndex * 2) > (LgSquare.Index - 1) )
    if( (TestIndex << 1) > (Square.GetIndex() - 1) )
      {
      ToMatch = Square.GetD( Square.GetIndex());
      }
    else
      {
      // LgSquare.Index is at least 2 here.
      ToMatch = Square.GetD( Square.GetIndex()) << 32;
      ToMatch |= Square.GetD( Square.GetIndex() - 1 );
      }

    SqrRoot.SetD( TestIndex, FindULSqrRoot( ToMatch ));
    TestIndex--;
    while( true )
      {
      // TestSqrtBits( TestIndex, LgSquare, LgSqrRoot );
      SearchSqrtXPart( TestIndex, Square, SqrRoot );
      if( TestIndex == 0 )
        break;

      TestIndex--;
      }

    // Avoid squaring the whole thing to see if it's an exact square root:
    if( ((SqrRoot.GetD( 0 ) * SqrRoot.GetD( 0 )) & 0xFFFFFFFF) != Square.GetD( 0 ))
      return false;

    TestForSquareRoot.Copy( SqrRoot );
    DoSquare( TestForSquareRoot );
    if( Square.IsEqual( TestForSquareRoot ))
      return true;
    else
      return false;

    }



  private void SearchSqrtXPart( int TestIndex, Integer Square, Integer SqrRoot )
    {
    // B is the Big part of the number that has already been found.
    // S = (B + x)^2
    // S = B^2 + 2Bx + x^2
    // S - B^2 = 2Bx + x^2
    // R = S - B^2
    // R = 2Bx + x^2
    // R = x(2B + x)
    SqrtXPartTest1.Copy( SqrRoot ); // B
    DoSquare( SqrtXPartTest1 ); // B^2
    SqrtXPartDiff.Copy( Square );
    Subtract( SqrtXPartDiff, SqrtXPartTest1 ); // S - B^2
    SqrtXPartTwoB.Copy( SqrRoot ); // B
    SqrtXPartTwoB.ShiftLeft( 1 ); // Times 2 for 2B.
    SqrtXPartTest1.Copy( SqrtXPartTwoB );
    ulong TestBits = SqrtXPartTest1.GetD( SqrtXPartTest1.GetIndex());
    int ShiftBy = Divider.FindShiftBy( TestBits );
    SqrtXPartR2.Copy( SqrtXPartDiff );
    SqrtXPartR2.ShiftLeft( ShiftBy );     // Multiply the numerator and the denominator
    SqrtXPartTest1.ShiftLeft( ShiftBy ); // by the same amount.
    ulong Highest;
    if( SqrtXPartR2.GetIndex() == 0 )
      {
      Highest = SqrtXPartR2.GetD( SqrtXPartR2.GetIndex());
      }
    else
      {
      Highest = SqrtXPartR2.GetD( SqrtXPartR2.GetIndex()) << 32;
      Highest |= SqrtXPartR2.GetD( SqrtXPartR2.GetIndex() - 1 );
      }

    ulong Denom = SqrtXPartTest1.GetD( SqrtXPartTest1.GetIndex());
    if( Denom == 0 )
      Highest = 0xFFFFFFFF;
    else
      Highest = Highest / Denom;

    if( Highest == 0 )
      {
      SqrRoot.SetD( TestIndex, 0 );
      return;
      }

    if( Highest > 0xFFFFFFFF )
      Highest = 0xFFFFFFFF;

    uint BitTest = 0x80000000;
    ulong XDigit = 0;
    ulong TempXDigit = 0;
    for( int BitCount = 0; BitCount < 32; BitCount++ )
      {
      TempXDigit = XDigit | BitTest;
      if( TempXDigit > Highest )
        {
        BitTest >>= 1;
        continue;
        }

      SqrtXPartTest1.Copy( SqrtXPartTwoB );
      SqrtXPartTest1.SetD( TestIndex, TempXDigit ); // 2B + x
      SqrtXPartTest2.SetDigitAndClear( TestIndex, TempXDigit ); // Set X.
      MultiplyTop( SqrtXPartTest2, SqrtXPartTest1 );
      if( SqrtXPartTest2.ParamIsGreaterOrEq( SqrtXPartDiff ))
        XDigit |= BitTest; // Then keep the bit.

      BitTest >>= 1;
      }

    SqrRoot.SetD( TestIndex, XDigit );
    }



  /*
  internal void ModularPowerOld( Integer Result, Integer Exponent, Integer ModN )
    {
    if( Result.IsZero())
      return; // With Result still zero.

    if( Result.IsEqual( ModN ))
      {
      // It is congruent to zero % ModN.
      Result.SetToZero();
      return;
      }

    // Result is not zero at this point.
    if( Exponent.IsZero() )
      {
      Result.SetFromULong( 1 );
      return;
      }

    if( ModN.ParamIsGreater( Result ))
      {
      Divide( Result, ModN, Quotient, Remainder );
      Result.Copy( Remainder );
      }

    if( Exponent.IsEqualToULong( 1 ))
      {
      // Result stays the same.
      return;
      }

    XForModPower.Copy( Result );
    ExponentCopy.Copy( Exponent );
    Result.SetFromULong( 1 );
    while( !ExponentCopy.IsZero())
      {
      // If the bit is 1, then do a lot more work here.
      if( (ExponentCopy.GetD( 0 ) & 1) == 1 )
        {
        // This is a multiplication for every _bit_.  So a 1024-bit
        // modulus means this gets called roughly 512 times.
        // The commonly used public exponent is 65537, which has
        // only two bits set to 1, the rest are all zeros.  But the
        // private key exponents are long randomish numbers.
        // (See: Hamming Weight.)
        Multiply( Result, XForModPower );
        SubtractULong( ExponentCopy, 1 );
        // Usually it's true that the Result is greater than ModN.
        if( ModN.ParamIsGreater( Result ))
          {
          // Here is where that really long division algorithm gets used a
          // lot in a loop.  And this Divide() gets called roughly about
          // 512 times.
          Divide( Result, ModN, Quotient, Remainder );
          Result.Copy( Remainder );
          }
        }

      // Square it.
      // This is a multiplication for every _bit_.  So a 1024-bit
      // modulus means this gets called 1024 times.
      Multiply( XForModPower, XForModPower );
      ExponentCopy.ShiftRight( 1 ); // Divide by 2.
      if( ModN.ParamIsGreater( XForModPower ))
        {
        // And this Divide() gets called about 1024 times.
        Divide( XForModPower, ModN, Quotient, Remainder );
        XForModPower.Copy( Remainder );
        }
      }
    }
    */



  internal void GreatestCommonDivisor( Integer X, Integer Y, Integer Gcd )
    {
    // This is the basic Euclidean Algorithm.
    if( X.IsZero())
      throw( new Exception( "Doing GCD with a parameter that is zero." ));

    if( Y.IsZero())
      throw( new Exception( "Doing GCD with a parameter that is zero." ));

    if( X.IsEqual( Y ))
      {
      Gcd.Copy( X );
      return;
      }

    // Don't change the original numbers that came in as parameters.
    if( X.ParamIsGreater( Y ))
      {
      GcdX.Copy( Y );
      GcdY.Copy( X );
      }
    else
      {
      GcdX.Copy( X );
      GcdY.Copy( Y );
      }

    while( true )
      {
      Divider.Divide( GcdX, GcdY, Quotient, Remainder );
      if( Remainder.IsZero())
        {
        Gcd.Copy( GcdY ); // It's the smaller one.
        // It can't return from this loop until the remainder is zero.
        return;
        }

      GcdX.Copy( GcdY );
      GcdY.Copy( Remainder );
      }
    }



  internal bool MultiplicativeInverse( Integer X, Integer Modulus, Integer MultInverse, BackgroundWorker Worker )
    {
    // This is the extended Euclidean Algorithm.
    // A*X + B*Y = Gcd
    // A*X + B*Y = 1 If there's a multiplicative inverse.
    // A*X = 1 - B*Y so A is the multiplicative inverse of X mod Y.
    if( X.IsZero())
      throw( new Exception( "Doing Multiplicative Inverse with a parameter that is zero." ));

    if( Modulus.IsZero())
      throw( new Exception( "Doing Multiplicative Inverse with a parameter that is zero." ));

    // This happens sometimes:
    // if( Modulus.ParamIsGreaterOrEq( X ))
      // throw( new Exception( "Modulus.ParamIsGreaterOrEq( X ) for Euclid." ));

    // Worker.ReportProgress( 0, " " );
    // Worker.ReportProgress( 0, " " );
    // Worker.ReportProgress( 0, "Top of mod inverse." );
    // U is the old part to keep.
    U0.SetToZero();
    U1.SetToOne();
    U2.Copy( Modulus ); // Don't change the original numbers that came in as parameters.
    // V is the new part.
    V0.SetToOne();
    V1.SetToZero();
    V2.Copy( X );
    T0.SetToZero();
    T1.SetToZero();
    T2.SetToZero();
    Quotient.SetToZero();
    // while( not forever if there's a problem )
    for( int Count = 0; Count < 10000; Count++ )
      {
      if( U2.IsNegative )
        throw( new Exception( "U2 was negative." ));

      if( V2.IsNegative )
        throw( new Exception( "V2 was negative." ));

      Divider.Divide( U2, V2, Quotient, Remainder );
      if( Remainder.IsZero())
        {
        Worker.ReportProgress( 0, "Remainder is zero. No multiplicative-inverse." );
        return false;
        }

      TempEuclid1.Copy( U0 );
      TempEuclid2.Copy( V0 );
      Multiply( TempEuclid2, Quotient );
      Subtract( TempEuclid1, TempEuclid2 );
      T0.Copy( TempEuclid1 );
      TempEuclid1.Copy( U1 );
      TempEuclid2.Copy( V1 );
      Multiply( TempEuclid2, Quotient );
      Subtract( TempEuclid1, TempEuclid2 );
      T1.Copy( TempEuclid1 );
      TempEuclid1.Copy( U2 );
      TempEuclid2.Copy( V2 );
      Multiply( TempEuclid2, Quotient );
      Subtract( TempEuclid1, TempEuclid2 );
      T2.Copy( TempEuclid1 );
      U0.Copy( V0 );
      U1.Copy( V1 );
      U2.Copy( V2 );
      V0.Copy( T0 );
      V1.Copy( T1 );
      V2.Copy( T2 );
      if( Remainder.IsOne())
        {
        // Worker.ReportProgress( 0, " " );
        // Worker.ReportProgress( 0, "Remainder is 1. There is a multiplicative-inverse." );
        break;
        }
      }

    MultInverse.Copy( T0 );
    if( MultInverse.IsNegative )
      {
      Add( MultInverse, Modulus );
      }

    // Worker.ReportProgress( 0, "MultInverse: " + ToString10( MultInverse ));
    TestForModInverse1.Copy( MultInverse );
    TestForModInverse2.Copy( X );
    Multiply( TestForModInverse1, TestForModInverse2 );
    Divider.Divide( TestForModInverse1, Modulus, Quotient, Remainder );
    if( !Remainder.IsOne())  // By the definition of Multiplicative inverse:
      throw( new Exception( "MultInverse is wrong: " + ToString10( Remainder )));

    // Worker.ReportProgress( 0, "MultInverse is the right number: " + ToString10( MultInverse ));
    return true;
    }



  internal bool IsFermatPrime( Integer ToTest, int HowMany )
    {
    // Also see Rabin-Miller test.
    // Also see Solovay-Strassen test.
    // Use bigger primes for Fermat test because the
    // modulus can't be too small.  And also it's
    // more likely to be congruent to 1 with a very
    // small modulus.  In other words it's a lot more
    // likely to appear to be a prime when it isn't.
    // This Fermat primality test is usually
    // described as using random primes to test with,
    // and you could do it that way too.  Except that
    // this Fermat test is being used to find random
    // primes, so...
    // A common way of doing this is to use a multiple
    // of several primes as the base, like
    // 2 * 3 * 5 * 7 = 210.
    int StartAt = IntegerMath.PrimeArrayLength - (1024 * 16); // Or much bigger.
    if( StartAt < 100 )
      StartAt = 100;

    for( int Count = StartAt; Count < (HowMany + StartAt); Count++ )
      {
      if( !IsFermatPrimeForOneValue( ToTest, PrimeArray[Count] ))
        return false;

      }

    // It _might_ be a prime if it passed this test.
    // Increasing HowMany increases the probability that it's a prime.
    return true;
    }



  // http://en.wikipedia.org/wiki/Primality_test
  // http://en.wikipedia.org/wiki/Fermat_primality_test
  internal bool IsFermatPrimeForOneValue( Integer ToTest, ulong Base )
    {
    // This won't catch Carmichael numbers.
    // http://en.wikipedia.org/wiki/Carmichael_number
    // Assume ToTest is not a small number.  (Not the size of a small prime.)
    // Normally it would be something like a 1024 bit number or bigger,
    // but I assume it's at least bigger than a 32 bit number.
    // Assume this has already been checked to see if it's divisible
    // by a small prime.
    // A has to be coprime to P and it is here because ToTest is not
    // divisible by a small prime.
    // Fermat's little theorem:
    // A ^ (P - 1) is congruent to 1 mod P if P is a prime.
    // Or: A^P - A is congrunt to A mod P.
    // If you multiply A by itself P times then divide it by P,
    // the remainder is A.  (A^P / P)
    // 5^3 = 125.  125 - 5 = 120.  A multiple of 5.
    // 2^7 = 128.  128 - 2 = 7 * 18 (a multiple of 7.)
    Fermat1.Copy( ToTest );
    SubtractULong( Fermat1, 1 );
    TestFermat.SetFromULong( Base );

    // ModularPower( Result, Exponent, Modulus, UsePresetBaseArray )
    ModularPower( TestFermat, Fermat1, ToTest, false );
    // if( !TestFermat.IsEqual( Fermat2 ))
      // throw( new Exception( "!TestFermat.IsEqual( Fermat2 )." ));

    if( TestFermat.IsOne())
      return true; // It passed the test. It _might_ be a prime.
    else
      return false; // It is _definitely_ a composite number.

    }



  internal ulong GetFactorial( uint Value )
    {
    if( Value == 0 )
      return 1;

    if( Value == 1 )
      return 1;

    uint Factorial = 1;
    for( uint Count = 2; Count <= Value; Count++ )
      Factorial = Factorial * Count;

    return Factorial;
    }



  internal string ShowBinomialCoefficients( uint Exponent )
    {
    try
    {
    // The expansion of: (X + Y)^N
    // A binomial coefficient is  N! / (K!*(N - K)!).
    // 0 raised to the 0 power is 1.  It is defined that way.
    // So (0 + 2)^3 has all terms set to zero except for 2^3.
    StringBuilder SBuilder = new StringBuilder();
    ulong ExponentFactorial = GetFactorial( Exponent );
    SBuilder.Append( "ExponentFactorial: " + ExponentFactorial.ToString() + "\r\n" );
    for( uint Count = 0; Count <= Exponent; Count++ )
      {
      uint K = Count;
      SBuilder.Append( "\r\n" );
      SBuilder.Append( "K: " + K.ToString() + "\r\n" );
      ulong KFactorial = GetFactorial( K );
      uint ExponentMinusK = (uint)((int)Exponent - (int)K);
      ulong ExponentMinusKFactorial = GetFactorial( ExponentMinusK );
      ulong Denom = KFactorial * ExponentMinusKFactorial;
      ulong Coefficient = ExponentFactorial / Denom;
      SBuilder.Append( "Coefficient: " + Coefficient.ToString() + "\r\n");
      }

    return SBuilder.ToString();
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in ShowBinomialCoefficients()." + Except.Message ));
      }
    }



  // This is the standard modular power algorithm that
  // you could find in any reference, but its use of
  // the new modular reduction algorithm is new (in 2015).
  // The square and multiply method is in Wikipedia:
  // https://en.wikipedia.org/wiki/Exponentiation_by_squaring
  // x^n = (x^2)^((n - 1)/2) if n is odd.
  // x^n = (x^2)^(n/2)       if n is even.
  internal void ModularPower( Integer Result, Integer Exponent, Integer Modulus, bool UsePresetBaseArray )
    {
    if( Result.IsZero())
      return; // With Result still zero.

    if( Result.IsEqual( Modulus ))
      {
      // It is congruent to zero % ModN.
      Result.SetToZero();
      return;
      }

    // Result is not zero at this point.
    if( Exponent.IsZero() )
      {
      Result.SetFromULong( 1 );
      return;
      }

    if( Modulus.ParamIsGreater( Result ))
      {
      // throw( new Exception( "This is not supposed to be input for RSA plain text." ));
      Divider.Divide( Result, Modulus, Quotient, Remainder );
      Result.Copy( Remainder );
      }

    if( Exponent.IsOne())
      {
      // Result stays the same.
      return;
      }

    if( !UsePresetBaseArray )
      SetupGeneralBaseArray( Modulus );

    XForModPower.Copy( Result );
    ExponentCopy.Copy( Exponent );
    // int TestIndex = 0;
    Result.SetFromULong( 1 );
    while( true )
      {
      if( (ExponentCopy.GetD( 0 ) & 1) == 1 ) // If the bottom bit is 1.
        {
        Multiply( Result, XForModPower );

        // if( Result.ParamIsGreater( CurrentModReductionBase ))
        // TestForModReduction2.Copy( Result );

        ModularReduction( TempForModPower, Result );
        // ModularReduction2( TestForModReduction2ForModPower, TestForModReduction2 );
        // if( !TestForModReduction2ForModPower.IsEqual( TempForModPower ))
          // {
          // throw( new Exception( "Mod Reduction 2 is not right." ));
          // }

        Result.Copy( TempForModPower );
        }

      ExponentCopy.ShiftRight( 1 ); // Divide by 2.
      if( ExponentCopy.IsZero())
        break;

      // Square it.
      Multiply( XForModPower, XForModPower );

      // Time this.
      // if( XForModPower.ParamIsGreater( CurrentModReductionBase ))
      ModularReduction( TempForModPower, XForModPower );
      XForModPower.Copy( TempForModPower );
      }

    // When ModularReduction() gets called it multiplies a base number
    // by a uint sized digit.  So that can make the result one digit bigger
    // than GeneralBase.  Then when they are added up you can get carry
    // bits that can make it a little bigger.
    int HowBig = Result.GetIndex() - Modulus.GetIndex();
    // if( HowBig > 1 )
      // throw( new Exception( "This does happen. Diff: " + HowBig.ToString() ));

    if( HowBig > 2 )
      throw( new Exception( "The never happens. Diff: " + HowBig.ToString() ));

    ModularReduction( TempForModPower, Result );
    Result.Copy( TempForModPower );

    // Notice that this Divide() is done once.  Not
    // a thousand or two thousand times.
    Divider.Divide( Result, Modulus, Quotient, Remainder );
    Result.Copy( Remainder );
    if( Quotient.GetIndex() > 1 )
      throw( new Exception( "This never happens. The quotient index is never more than 1." ));

    }


/*
  // Copyright Eric Chauvin 2015 - 2018.
  private int ModularReduction( Integer Result, Integer ToReduce )
    {
    try
    {
    if( GeneralBaseArray == null )
      throw( new Exception( "SetupGeneralBaseArray() should have already been called." ));

    Result.SetToZero();
    int HowManyToAdd = ToReduce.GetIndex() + 1;
    if( HowManyToAdd > GeneralBaseArray.Length )
      throw( new Exception( "The Input number should have been reduced first. HowManyToAdd > GeneralBaseArray.Length" ));

    int BiggestIndex = 0;
    for( int Count = 0; Count < HowManyToAdd; Count++ )
      {
      // The size of the numbers in GeneralBaseArray are
      // all less than the size of GeneralBase.
      // This multiplication by a uint is with a number
      // that is not bigger than GeneralBase.  Compare
      // this with the two full Muliply() calls done on
      // each digit of the quotient in LongDivide3().
      // AccumulateBase is set to a new value here.
      int CheckIndex = MultiplyUIntFromCopy( AccumulateBase, GeneralBaseArray[Count], ToReduce.GetD( Count ));
      if( CheckIndex > BiggestIndex )
        BiggestIndex = CheckIndex;

      Result.Add( AccumulateBase );
      }

    return Result.GetIndex();
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in ModularReduction(): " + Except.Message ));
      }
    }
*/



  // Copyright Eric Chauvin 2015 - 2018.
  private int ModularReduction( Integer Result, Integer ToReduce )
    {
    try
    {
    if( ToReduce.ParamIsGreater( CurrentModReductionBase ))
      {
      Result.Copy( ToReduce );
      return Result.GetIndex();
      }

    if( GeneralBaseArray == null )
      throw( new Exception( "SetupGeneralBaseArray() should have already been called." ));

    Result.SetToZero();
    int TopOfToReduce = ToReduce.GetIndex() + 1;
    if( TopOfToReduce > GeneralBaseArray.Length )
      throw( new Exception( "The Input number should have been reduced first. HowManyToAdd > GeneralBaseArray.Length" ));

    // If it gets this far then ToReduce is at
    // least this big.
    int HighestCopyIndex = CurrentModReductionBase.GetIndex();
    for( int Count = 0; Count < HighestCopyIndex; Count++ )
      Result.SetD( Count, ToReduce.GetD( Count ));

    Result.SetIndex( HighestCopyIndex - 1 );

    int BiggestIndex = 0;
    for( int Count = HighestCopyIndex; Count < TopOfToReduce; Count++ )
      {
      // The size of the numbers in GeneralBaseArray
      // are all less than the size of GeneralBase.
      // This multiplication by a uint is with a
      // number that is not bigger than GeneralBase.
      // Compare this with the two full Muliply()
      // calls done on each digit of the quotient
      // in LongDivide3().

      // AccumulateBase is set to a new value here.
      int CheckIndex = MultiplyUIntFromCopy( AccumulateBase, GeneralBaseArray[Count], ToReduce.GetD( Count ));
      if( CheckIndex > BiggestIndex )
        BiggestIndex = CheckIndex;

      Result.Add( AccumulateBase );
      }

    return Result.GetIndex();
    }
    catch( Exception Except )
      {
      throw( new Exception( "Exception in ModularReduction(): " + Except.Message ));
      }
    }



  internal void SetupGeneralBaseArray( Integer GeneralBase )
    {
    // The word 'Base' comes from the base of a number
    // system.  Like normal decimal numbers have base
    // 10, binary numbers have base 2, etc.

    CurrentModReductionBase.Copy( GeneralBase );

    // The input to the accumulator can be twice the bit length of GeneralBase.
    int HowMany = ((GeneralBase.GetIndex() + 1) * 2) + 10; // Plus some extra for carries...

    if( GeneralBaseArray == null )
      {
      GeneralBaseArray = new Integer[HowMany];
      }

    if( GeneralBaseArray.Length < HowMany )
      {
      GeneralBaseArray = new Integer[HowMany];
      }

    Integer Base = new Integer();
    Base.SetFromULong( 256 ); // 0x100
    MultiplyUInt( Base, 256 ); // 0x10000
    MultiplyUInt( Base, 256 ); // 0x1000000
    MultiplyUInt( Base, 256 ); // 0x100000000 is the base of this number system.
    // 0x1 0000 0000

    Integer BaseValue = new Integer();
    BaseValue.SetFromULong( 1 );

    for( int Count = 0; Count < HowMany; Count++ )
      {
      if( GeneralBaseArray[Count] == null )
        GeneralBaseArray[Count] = new Integer();

      Divider.Divide( BaseValue, GeneralBase, Quotient, Remainder );
      GeneralBaseArray[Count].Copy( Remainder );

      BaseValue.Copy( Remainder );
      Multiply( BaseValue, Base );
      }
    }



  internal uint ModularPowerSmall( ulong Input, Integer Exponent, uint Modulus )
    {
    if( Input == 0 )
      return 0;

    if( Input == Modulus )
      {
      // It is congruent to zero % Modulus.
      return 0;
      }

    // Result is not zero at this point.
    if( Exponent.IsZero() )
      return 1;

    ulong Result = Input;
    if( Input > Modulus )
      Result = Input % Modulus;

    if( Exponent.IsOne())
      return (uint)Result;

    ulong XForModPowerU = Result;
    ExponentCopy.Copy( Exponent );
    // int TestIndex = 0;
    Result = 1;
    while( true )
      {
      if( (ExponentCopy.GetD( 0 ) & 1) == 1 ) // If the bottom bit is 1.
        {
        Result = Result * XForModPowerU;
        Result = Result % Modulus;
        }

      ExponentCopy.ShiftRight( 1 ); // Divide by 2.
      if( ExponentCopy.IsZero())
        break;

      // Square it.
      XForModPowerU = XForModPowerU * XForModPowerU;
      XForModPowerU = XForModPowerU % Modulus;
      }

    return (uint)Result;
    }



  internal bool FindMultiplicativeInverseSmall( Integer ToFind, Integer KnownNumber, Integer Modulus, BackgroundWorker Worker )
    {
    // This method is for: KnownNumber * ToFind = 1 mod Modulus
    // An example:
    // PublicKeyExponent * X = 1 mod PhiN.
    // PublicKeyExponent * X = 1 mod (P - 1)(Q - 1).
    // This means that
    // (PublicKeyExponent * X) = (Y * PhiN) + 1
    // X is less than PhiN.
    // So Y is less than PublicKExponent.
    // Y can't be zero.
    // If this equation can be solved then it can be solved modulo
    // any number.  So it has to be solvable mod PublicKExponent.
    // See: Hasse Principle.
    // This also depends on the idea that the KnownNumber is prime and
    // that there is one unique modular inverse.
    // if( !KnownNumber-is-a-prime )
    //    then it won't work.
    if( !KnownNumber.IsULong())
      throw( new Exception( "FindMultiplicativeInverseSmall() was called with too big of a KnownNumber." ));

    ulong KnownNumberULong  = KnownNumber.GetAsULong();
    //                       65537
    if( KnownNumberULong > 1000000 )
      throw( new Exception( "KnownNumberULong > 1000000. FindMultiplicativeInverseSmall() was called with too big of an exponent." ));

    // (Y * PhiN) + 1 mod PubKExponent has to be zero if Y is a solution.
    ulong ModulusModKnown = Divider.GetMod32( Modulus, KnownNumberULong );
    Worker.ReportProgress( 0, "ModulusModExponent: " + ModulusModKnown.ToString( "N0" ));
    if( Worker.CancellationPending )
      return false;

    // Y can't be zero.
    // The exponent is a small number like 65537.
    for( uint Y = 1; Y < (uint)KnownNumberULong; Y++ )
      {
      ulong X = (ulong)Y * ModulusModKnown;
      X++; // Add 1 to it for (Y * PhiN) + 1.
      X = X % KnownNumberULong;
      if( X == 0 )
        {
        if( Worker.CancellationPending )
          return false;

        // What is PhiN mod 65537?
        // That gives me Y.
        // The private key exponent is X*65537 + ModPart
        // The CipherText raised to that is the PlainText.
        // P + zN = C^(X*65537 + ModPart)
        // P + zN = C^(X*65537)(C^ModPart)
        // P + zN = ((C^65537)^X)(C^ModPart)
        Worker.ReportProgress( 0, "Found Y at: " + Y.ToString( "N0" ));
        ToFind.Copy( Modulus );
        MultiplyULong( ToFind, Y );
        ToFind.AddULong( 1 );
        Divider.Divide( ToFind, KnownNumber, Quotient, Remainder );
        if( !Remainder.IsZero())
          throw( new Exception( "This can't happen. !Remainder.IsZero()" ));

        ToFind.Copy( Quotient );
        // Worker.ReportProgress( 0, "ToFind: " + ToString10( ToFind ));
        break;
        }
      }

    if( Worker.CancellationPending )
      return false;

    TestForModInverse1.Copy( ToFind );
    MultiplyULong( TestForModInverse1, KnownNumberULong );
    Divider.Divide( TestForModInverse1, Modulus, Quotient, Remainder );
    if( !Remainder.IsOne())
      {
      // The definition is that it's congruent to 1 mod the modulus,
      // so this has to be 1.
      // I've only seen this happen once.  Were the primes P and Q not
      // really primes?
      throw( new Exception( "Remainder has to be 1: " + ToString10( Remainder ) ));
      }

    return true;
    }



  internal int GetMaxModPowerIndex()
    {
    return MaxModPowerIndex;
    }



  }
}








