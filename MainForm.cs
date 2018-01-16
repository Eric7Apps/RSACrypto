// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


// RSACrypto


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Diagnostics; // For start program process.
using System.Security.Cryptography;
// CryptoGraphic Services:
// https://msdn.microsoft.com/en-us/library/92f9ye3s%28v=vs.110%29.aspx
// using System.Net.Security; // SslStream
// using System.Security;
// ICryptoTransform
// using System.Security.Cryptography.X509Certificates;


namespace RSACrypto
{

    /*
  internal struct QuadResWorkerInfo
    {
    // internal CRTCombinSetupRec[] SetupArray;
    internal string PublicKeyModulus;
    internal uint ModMask;
    internal string ProcessName;
    } */



  internal struct RSACryptoWorkerInfo
    {
    internal string ProcessName;
    }


  public partial class MainForm : Form
  {
  internal const string VersionDate = "1/16/2018";
  internal const int VersionNumber = 09; // 0.9
  // private System.Threading.Mutex SingleInstanceMutex = null;
  // private bool IsSingleInstance = false;
  private bool IsClosing = false;
  private bool Cancelled = false;
  internal const string MessageBoxTitle = "RSA Crypto";
  // private string DataDirectory = "";
  // private bool FormShownOnce = false;
  // private QuadResCombinBackground[] QuadResCombinBackgArray;
  private RSACryptoBackground RSACryptoBack;
  // private ExpVectorBackground[] ExpVectorArray;


  public MainForm()
    {
    InitializeComponent();
    this.Font = new System.Drawing.Font( "Consolas", 34.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    ShowStatus( "Version date: " + VersionDate );
    }



  internal bool GetIsClosing()
    {
    return IsClosing;
    }



  private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
    Close();
    }



  private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    string ShowS = "Programming by Eric Chauvin." +
            " Version date: " + VersionDate;

    MessageBox.Show(ShowS, MessageBoxTitle, MessageBoxButtons.OK);
    }



  internal void ShowStatus(string Status)
    {
    if (IsClosing)
      return;

    // if( StatusTextBox.Text.Length > (80 * 5000))
      // StatusTextBox.Text = "";

    StatusTextBox.AppendText( Status + "\r\n" );
    }



  internal void ClearStatus()
    {
    if (IsClosing)
      return;

    StatusTextBox.Text = "";
    }



  private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
    if( e.KeyCode == Keys.Escape ) //  && (e.Alt || e.Control || e.Shift))
      {
      ShowStatus( "Cancelled." );
      Cancelled = true;
      FreeEverything(); // Stop background process.
      }
    }



  private void makeKeysToolStripMenuItem_Click(object sender, EventArgs e)
    {
    Cancelled = false;
    RSACryptoWorkerInfo WInfo = new RSACryptoWorkerInfo();
    WInfo.ProcessName = "RSA Make Keys";
    RSACryptoBack = new RSACryptoBackground( this, WInfo );
    RSACryptoBack.RunWorkerAsync( WInfo );
    }



  private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
    //  if( IsSingleInstance )
    //      {
    if( DialogResult.Yes != MessageBox.Show( "Close the program?", MessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question ))
      {
      e.Cancel = true;
      return;
      }

    IsClosing = true;
    FreeEverything();
    }



  internal void FreeEverything()
    {
    if( IsDisposed )
      return;

    if( RSACryptoBack != null )
      {
      if( RSACryptoBack.IsBusy )
        {
        if( !RSACryptoBack.CancellationPending )
          RSACryptoBack.CancelAsync();

        }

      RSACryptoBack.Dispose();
      RSACryptoBack = null;
      }

    /*
    if( QuadResCombinBackgArray != null )
      {
      try
      {
      for( int Count = 0; Count < QuadResCombinBackgArray.Length; Count++ )
        {
        if( QuadResCombinBackgArray[Count] == null )
          continue;

        if( QuadResCombinBackgArray[Count].IsBusy )
          {
          if( !QuadResCombinBackgArray[Count].CancellationPending )
            QuadResCombinBackgArray[Count].CancelAsync();

        }

      QuadResCombinBackgArray[Count].Dispose();
      QuadResCombinBackgArray[Count] = null;
      }
      }
      catch( Exception Except )
        {
        ShowStatus( "Exception for QuadResCombinBackgArray[] on closing:\r\n" + Except.Message );
        }
      }
      */

    }




  internal bool CheckEvents()
    {
    if( IsClosing )
      return false;

    Application.DoEvents();
    if( Cancelled )
      return false;

    return true;
    }




    /*
    ================
Things I'll fix up later.






      internal int GetRandomNumber()
        {
        byte[] TheBytes = new byte[4];
        CryptoRand.GetBytes( TheBytes );
        uint Result = (uint)(TheBytes[0]) & 0x7F;
        Result <<= 8;
        Result |= TheBytes[1];
        Result <<= 8;
        Result |= TheBytes[2];
        Result <<= 8;
        Result |= TheBytes[3];
        return (int)Result;
        }



      internal void FoundSolutionShutDown( string ProcessName,
                                           string SolutionP,
                                           string SolutionQ )
        {
        if( IsClosing )
          return;
        if( MakeKeysForm == null )
          return;
        if( MakeKeysForm.IsDisposed )
          return;
        MakeKeysForm.FoundSolutionShutDown( ProcessName, SolutionP, SolutionQ );
        }




      internal void FoundSolutionShutDownQuadRes( string ProcessName,
                                           string SolutionP,
                                           string SolutionQ )
        {
        if( IsClosing )
          return;
        if( QuadResForm == null )
          return;
        if( QuadResForm.IsDisposed )
          return;
        QuadResForm.FoundSolutionShutDown( ProcessName, SolutionP, SolutionQ );
        }



      private void testToolStripMenuItem_Click( object sender, EventArgs e )
        {
        Cancelled = false;
        try
        {
        // Some test numbers:
        // string PubKey =                                    "429,606,691,379";
        // string PubKey =                                  "9,073,276,189,777";
        // string PubKey =                            "243,047,686,576,334,411";
        // string PubKey =                     "15,654,675,243,543,711,381,029";
        // string PubKey =                 "28,569,209,393,580,650,250,552,443";
        // string PubKey =              "1,646,567,279,211,844,205,225,474,329";
        // Process 0) Hours: 0  Minutes: 27  Seconds: 8
        // Four cores. Process 1) Hours: 0  Minutes: 25  Seconds: 10
        // SolutionP: 35,138,851,773,149
        // SolutionQ: 61,800,192,159,577
        // To 12.
        // string PubKey =              "2,171,587,791,847,501,194,011,797,973";   // To 12.
        // Process 0) Hours: 0  Minutes: 10  Seconds: 47
        // SolutionP: 42,244,231,868,831
        // SolutionQ: 228,244,523,296,981
        string PubKey =              "9,642,014,564,948,464,387,250,299,211";
        // Process 0) Hours: 0  Minutes: 24  Seconds: 26
        //            123 456 789 012 345
        // SolutionP: 135,206,875,645,343
        // SolutionQ: 233,212,212,644,977
        // string PubKey =             "31,531,894,634,064,693,561,822,392,111";
        // Process 1) Hours: 0  Minutes: 4  Seconds: 1
        // Process 1) Hours: 0  Minutes: 3  Seconds: 39
        // SolutionP: 210,661,490,216,893
        // SolutionQ: 268,137,318,741,769
        // string PubKey =             "56,486,207,148,903,090,249,668,503,717";
        // Process 0) Hours: 0  Minutes: 7  Seconds: 21
        // SolutionP: 939,837,758,391,053
        // SolutionQ: 962,532,058,238,111
        //                                         1            2            3
        //                             123 456 789 012 345 678 901 234 567 890
        // string PubKey =            "904,623,971,994,032,721,365,326,020,883";
        // Process 1) Hours: 1  Minutes: 33  Seconds: 57
        // There are 8,760 hours per year.
        // Process 1) Hours: 1  Minutes: 6  Seconds: 14
        // SolutionP: 3,578,462,858,032,427
        // SolutionQ: 4,297,023,441,740,867
        // To 13:
        //                                      1            2             3
        //                          12 345 678 901 234 567 890 123 456 789 012
        // string PubKey =         "15,376,738,786,364,358,999,363,217,094,209";
        // string PubKey =         "16,682,443,477,710,019,133,059,293,108,983";
        // string PubKey =         "24,301,634,242,369,530,359,250,929,879,449";
        // string PubKey =        "515,597,628,428,905,809,723,177,634,351,747";
        // string PubKey =     "14,887,421,987,705,857,342,258,870,638,793,433";
        // string PubKey =     "23,315,998,853,571,458,231,621,674,575,432,869";
        // string PubKey =    "356,119,202,246,235,087,093,109,209,643,317,503";
        // string PubKey =  "2,322,646,449,067,527,436,179,659,402,038,526,369";
        //                              1            2             3            4
        //                  12 345 678 901 234 567 890 123 456 789 012 345 678
        // string PubKey = "31,812,176,461,304,096,446,192,328,013,544,627,573";
        // string PubKey = "966,789,092,655,927,565,959,398,119,581,788,384," +
        //                "187,305,048,744,308,467,677";
        // string PubKey = "3,624,234,739,637,352,431,468,730,305,176,365,190," +
        //                "269,947,830,981,235,062,150,834,495,522,126,114,009";
        // RSA-100 is 100 decimal digits.
        // string PubKey = "15226050279225333605356183781326374297180681149613" +
        //                 "80688657908494580122963258952897654000350692006139";
        // RSA-2048

        // string PubKey = "2519590847565789349402718324004839857142928212620403202777713783604366202070" +
        //       "7595556264018525880784406918290641249515082189298559149176184502808489120072" +
        //       "8449926873928072877767359714183472702618963750149718246911650776133798590957" +
        //       "0009733045974880842840179742910064245869181719511874612151517265463228221686" +
        //       "9987549182422433637259085141865462043576798423387184774447920739934236584823" +
        //       "8242811981638150106748104516603773060562016196762561338441436038339044149526" +
        //       "3443219011465754445417842402092461651572335077870774981712577246796292638635" +
        //       "6373289912154831438167899885040445364023527381951378636564391212010397122822" +
        //       "120720357";

        MForm.FactorDictSetProduct( PubKey );
        // Three threads to run on 4 cores. My laptop has
        // two cores, where each core has one virtual core,
        // so its 4 logical cores.
        QuadResCombinBackgArray = new QuadResCombinBackground[3];
        // ExpVectorArray = new ExpVectorBackground[3];
        // Some primes:
        // 0  1  2  3   4   5   6   7   8   9  10  11  12  13  14  15
        // 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53,
        // 16  17  18  19  20  21  22  23  24   25   26   27   28   29
        // 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113,
        //  30   31   32   33   34   35   36   37   38   39   40   41
        // 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181,
        //  42   43   44   45   46   47   48   49   50   51   52   53
        // 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251,
        //  54   55   56   57   58   59   60   61   62   63   64   65
        // 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317,
        //  66   67   68   69   70   71   72   73   74   75   76   77
        // 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,


    ===
        QuadResWorkerInfo WInfo = new QuadResWorkerInfo();
        WInfo.PublicKeyModulus = PubKey;
        WInfo.ModMask = 1;
        WInfo.ProcessName = "Process 0";
        ExpVectorArray[0] = new ExpVectorBackground( MForm, WInfo );
        ShowStatus( "Starting background process at: 0" );
        ExpVectorArray[0].RunWorkerAsync( WInfo );

    ===
        QuadResWorkerInfo WInfo = new QuadResWorkerInfo();
        WInfo.PublicKeyModulus = PubKey;
        WInfo.ModMask = 0; // ModMask tells it what partition to check.
        WInfo.ProcessName = "Process 0";
        QuadResCombinBackgArray[0] = new QuadResCombinBackground( MForm, WInfo );
        ShowStatus( "Starting background process at: 0" );
        QuadResCombinBackgArray[0].RunWorkerAsync( WInfo );
        ////////////////
        WInfo = new QuadResWorkerInfo();
        WInfo.PublicKeyModulus = PubKey;
        WInfo.ModMask = 1;
        WInfo.ProcessName = "Process 1";
        QuadResCombinBackgArray[1] = new QuadResCombinBackground( MForm, WInfo );
        ShowStatus( "Starting background process at: 1" );
        QuadResCombinBackgArray[1].RunWorkerAsync( WInfo );
        ////////
        WInfo = new QuadResWorkerInfo();
        WInfo.PublicKeyModulus = PubKey;
        WInfo.ModMask = 2;
        WInfo.ProcessName = "Process 2";
        QuadResCombinBackgArray[2] = new QuadResCombinBackground( MForm, WInfo );
        ShowStatus( "Starting background process at: 2" );
        QuadResCombinBackgArray[2].RunWorkerAsync( WInfo );


        ////////
        WInfo = new QuadResWorkerInfo();
        WInfo.PublicKeyModulus = PubKey;
        WInfo.ModMask = 3;
        WInfo.ProcessName = "Process 3";
        QuadResCombinBackgArray[3] = new QuadResCombinBackground( MForm, WInfo );
        ShowStatus( "Starting background process at: 3" );
        QuadResCombinBackgArray[3].RunWorkerAsync( WInfo );

        }
        catch( Exception Except )
          {
          ShowStatus( "Exception in testToolStripMenuItem_Click():\r\n" + Except.Message );
          }
        }


      internal void FoundSolutionShutDown( string ProcessName,
                                           string SolutionP,
                                           string SolutionQ )
        {
        if( IsDisposed )
          return;
        ShowStatus( "Found the solution in: " + ProcessName );
        ShowStatus( "Stopping processes." );
        ShowStatus( "SolutionP: " + SolutionP );
        ShowStatus( "SolutionQ: " + SolutionQ );
        if( QuadResCombinBackgArray == null )
          return;
        for( int Count = 0; Count < QuadResCombinBackgArray.Length; Count++ )
          {
          try
          {
          if( QuadResCombinBackgArray[Count] == null )
            continue;
          if( QuadResCombinBackgArray[Count].IsBusy )
            {
            if( !QuadResCombinBackgArray[Count].CancellationPending )
              QuadResCombinBackgArray[Count].CancelAsync();
            }
          QuadResCombinBackgArray[Count].Dispose();
          QuadResCombinBackgArray[Count] = null;
          }
          catch( Exception Except )
            {
            ShowStatus( "Exception for FoundSolutionShutDown():\r\n" + Except.Message );
            }
          }
        }


    ==================
    */


  }
}






