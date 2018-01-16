// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace RSACrypto
{
  class RSACryptoBackground : BackgroundWorker
  {
  private MainForm MForm;
  // private string SolutionP = "";
  // private string SolutionQ = "";
  private string ProcessName = "No Name";



  private RSACryptoBackground()
    {
    }



  internal RSACryptoBackground( MainForm UseForm, RSACryptoWorkerInfo WInfo )
    {
    MForm = UseForm;
    DoWork += new DoWorkEventHandler( RSACryptoBackground_DoWork );
    ProgressChanged += new ProgressChangedEventHandler( RSACryptoBackground_ProgressChanged );
    RunWorkerCompleted += new RunWorkerCompletedEventHandler( RSACryptoBackground_RunWorkerCompleted );
    WorkerReportsProgress = true;
    WorkerSupportsCancellation = true;
    ProcessName = WInfo.ProcessName;
    }



  private void RSACryptoBackground_DoWork(object sender, DoWorkEventArgs e)
    {
    if( CancellationPending )
      return;

    if( MForm.GetIsClosing())
      return;

    BackgroundWorker Worker = (BackgroundWorker)sender;
    RSACryptoWorkerInfo WInfo = (RSACryptoWorkerInfo)(e.Argument);
    try // catch
    {
    if( Worker.CancellationPending )
      {
      e.Cancel = true;
      return;
      }

    RSACryptoSystem RSACrypto = new RSACryptoSystem( Worker, WInfo );
    RSACrypto.MakeRSAKeys();
    RSACrypto.FreeEverything();
    if( Worker.CancellationPending )
      {
      e.Cancel = true;
      return;
      }
    }
    catch( Exception Except )
      {
      Worker.ReportProgress( 0, "Error in RSACryptoBackground DoWork process:" );
      Worker.ReportProgress( 0, Except.Message );
      e.Cancel = true;
      }
    }



  private void RSACryptoBackground_ProgressChanged( object sender, ProgressChangedEventArgs e )
    {
    // This runs in the UI thread.
    if( CancellationPending )
      return;

    if( MForm.GetIsClosing())
      return;

    string CheckStatus = (string)e.UserState;
    if( CheckStatus == null )
      return;

    if( CheckStatus.Length < 1 )
      return;

    // if( e.ProgressPercentage > 0 )
    if( CheckStatus.Trim().Length < 1 )
      MForm.ShowStatus( CheckStatus );
    else
      MForm.ShowStatus( ProcessName + ") " + CheckStatus );

    }



  private void RSACryptoBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
    if( CancellationPending )
      return;

    if( MForm.GetIsClosing())
      return;

    if( e.Cancelled )
      {
      MForm.ShowStatus( "Cancelled." );
      return;
      }

    MForm.ShowStatus( "Finished RSACryptoBackground process." );
    }



  }
}

