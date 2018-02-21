// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com



using System;


namespace RSACrypto
{
  class ModularReduction
  {
  private MainForm MForm;
  private IntegerMath IntMath;
  private Integer[] GeneralBaseArray;
  // private uint[,] SmallBasesArray;
  private Integer Quotient = new Integer();
  private Integer Remainder = new Integer();
  private Integer CurrentModReductionBase = new Integer();
  private Integer AccumulateBase = new Integer();
  private Integer TempForModPower = new Integer();
  private Integer TestForModPower = new Integer();
  private Integer TestForModReduction2 = new Integer();
  private Integer TestForModReduction2ForModPower = new Integer();
  private Integer XForModPower = new Integer();
  private Integer ExponentCopy = new Integer();
  // private int MaxModPowerIndex = 0;




  private ModularReduction()
    {
    }


  internal ModularReduction( MainForm UseForm, IntegerMath UseIntMath )
    {
    MForm = UseForm;
    IntMath = UseIntMath;

    // You might want to pass a null IntMath to this
    // so that it creates its own that doesn't
    // interfere with something else.
    if( IntMath == null )
      IntMath = new IntegerMath( MForm );

    }



  internal void SetupGeneralBaseArray( Integer GeneralBase )
    {
    // The word 'Base' comes from the base of a number
    // system.  Like normal decimal numbers have base
    // 10, binary numbers have base 2, etc.

    CurrentModReductionBase.Copy( GeneralBase );

    // The input to the accumulator can be twice the
    // bit length of GeneralBase.
    int HowManyDigits = ((GeneralBase.GetIndex() + 1) * 2) + 10; // Plus some extra for carries...

    GeneralBaseArray = new Integer[HowManyDigits];

    // int HowManyPrimes = 100;
    //                         Row, Column
    // SmallBasesArray = new uint[HowManyDigits, HowManyPrimes];

    Integer Base = new Integer();
    Base.SetFromULong( 256 ); // 0x100
    IntMath.MultiplyUInt( Base, 256 ); // 0x10000
    IntMath.MultiplyUInt( Base, 256 ); // 0x1000000
    IntMath.MultiplyUInt( Base, 256 ); // 0x100000000 is the base of this number system.
    // 0x1 0000 0000

    Integer BaseValue = new Integer();
    BaseValue.SetFromULong( 1 );

    for( int Column = 0; Column < HowManyDigits; Column++ )
      {
      if( GeneralBaseArray[Column] == null )
        GeneralBaseArray[Column] = new Integer();

      IntMath.Divider.Divide( BaseValue, GeneralBase, Quotient, Remainder );
      GeneralBaseArray[Column].Copy( Remainder );

/*
      for( int Row = 0; Row < HowManyPrimes; Row++ )
        {
        This base value mod this prime?

        SmallBasesArray[Row, Column] = what?
        }
*/

      // Done at the bottom for the next round of the
      // loop.
      BaseValue.Copy( Remainder );
      IntMath.Multiply( BaseValue, Base );
      }
    }




  // Copyright Eric Chauvin 2015 - 2018.
  internal int Reduce( Integer Result, Integer ToReduce )
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
    Result.CopyUpTo( ToReduce, HighestCopyIndex - 1 );

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
      int CheckIndex = IntMath.MultiplyUIntFromCopy( AccumulateBase, GeneralBaseArray[Count], ToReduce.GetD( Count ));
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
      IntMath.Divider.Divide( Result, Modulus, Quotient, Remainder );
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
        IntMath.Multiply( Result, XForModPower );

        // if( Result.ParamIsGreater( CurrentModReductionBase ))
        // TestForModReduction2.Copy( Result );

        Reduce( TempForModPower, Result );
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
      IntMath.Multiply( XForModPower, XForModPower );

      // Time this.
      // if( XForModPower.ParamIsGreater( CurrentModReductionBase ))
      Reduce( TempForModPower, XForModPower );
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

    Reduce( TempForModPower, Result );
    Result.Copy( TempForModPower );

    // Notice that this Divide() is done once.  Not
    // a thousand or two thousand times.
/*
    Integer ResultTest = new Integer();
    Integer ModulusTest = new Integer();
    Integer QuotientTest = new Integer();
    Integer RemainderTest = new Integer();

    ResultTest.Copy( Result );
    ModulusTest.Copy( Modulus );
    IntMath.Divider.DivideForSmallQuotient( ResultTest,
                            ModulusTest,
                            QuotientTest,
                            RemainderTest );

*/

    IntMath.Divider.Divide( Result, Modulus, Quotient, Remainder );

    // if( !RemainderTest.IsEqual( Remainder ))
      // throw( new Exception( "DivideForSmallQuotient() !RemainderTest.IsEqual( Remainder )." ));

    // if( !QuotientTest.IsEqual( Quotient ))
      // throw( new Exception( "DivideForSmallQuotient() !QuotientTest.IsEqual( Quotient )." ));


    Result.Copy( Remainder );
    if( Quotient.GetIndex() > 1 )
      throw( new Exception( "This never happens. The quotient index is never more than 1." ));

    }


/*
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
*/


/*
  internal int GetMaxModPowerIndex()
    {
    return MaxModPowerIndex;
    }
*/



  }
}










