using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;

//See Service-based Database - Salim SR tutorial. Need internet to download pack. 


namespace Sophie
{
    public partial class Form1 : Form
    {
        SpeechRecognitionEngine speechreco = null;//new SpeechRecognitionEngine();
        SpeechSynthesizer sophie = new SpeechSynthesizer();

        //Grammar variables allow us to update and unload words into Sophie
        Grammar shellcommandgrammar, webcommandgrammar;
        //Using these to hold new data to be added
        String[] ArrayShellCommands;
        String[] ArrayShellResponses;
        String[] ArrayShellLocations;
        String[] ArraySocialCommands;
        String[] ArraySocialResponses;
        String[] ArrayUserSettings;
        StreamWriter sw;

        //Username, used for files
        public static String userName = Environment.UserName;

        public Form1()
        {
            InitializeComponent();
            try
            {
                speechreco = CreateSpeechEngine("en-US");
                speechreco.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(Speechreco_AudioRecognized);
                speechreco.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Speechreco_SpeechRecognized);
                loadGrammar();
                speechreco.SetInputToDefaultAudioDevice();
                speechreco.RecognizeAsync(RecognizeMode.Multiple);

                sophie.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(sophie_SpeakCompleted);
            }
            catch (Exception ex)
            {
                sophie.SpeakAsync("Voice recognition failed" + ex.Message);
            }

            //Create Directory and Commands
            Directory.CreateDirectory(@"C:\Users\" + userName + "\\Documents\\Sophie_Commands"); //TODO CONTINUE HERE
            Properties.Settings.Default.ShellC = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\Shell_Commands.txt";
            Properties.Settings.Default.ShellR = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\Shell_Response.txt";
            Properties.Settings.Default.ShellL = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\Shell_Location.txt";
            Properties.Settings.Default.SocialC = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\Social_Commands.txt";
            Properties.Settings.Default.SocialR = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\Social_Response.txt";
            Properties.Settings.Default.UserS = @"C:\Users\" + userName + "\\Documents\\Sophie_Commands\\User_Settings.txt";

            if (!File.Exists(Properties.Settings.Default.ShellC)) sw = File.CreateText(Properties.Settings.Default.ShellC);
            if (!File.Exists(Properties.Settings.Default.ShellR)) sw = File.CreateText(Properties.Settings.Default.ShellR);
            if (!File.Exists(Properties.Settings.Default.ShellL)) sw = File.CreateText(Properties.Settings.Default.ShellL);
            if (!File.Exists(Properties.Settings.Default.SocialC)) sw = File.CreateText(Properties.Settings.Default.SocialC);
            if (!File.Exists(Properties.Settings.Default.SocialR)) sw = File.CreateText(Properties.Settings.Default.SocialR);
            if (!File.Exists(Properties.Settings.Default.UserS)) sw = File.CreateText(Properties.Settings.Default.UserS);

            ArrayShellCommands = File.ReadAllLines(Properties.Settings.Default.ShellC);
            ArrayShellResponses = File.ReadAllLines(Properties.Settings.Default.ShellR);
            ArrayShellLocations = File.ReadAllLines(Properties.Settings.Default.ShellL);
            ArraySocialCommands = File.ReadAllLines(Properties.Settings.Default.SocialC);
            ArraySocialResponses = File.ReadAllLines(Properties.Settings.Default.SocialR);
            ArrayUserSettings = File.ReadAllLines(Properties.Settings.Default.UserS);

            // Currently user setup : 1st line = Name, 2nd. (not in use (date) ). 3rd is different voice settings
            // Inbuilt voice settings are : Microsoft Zira Desktop, Microsoft David Desktop, Microsoft Mark Desktop
            //If No user Data collected
            if (ArrayUserSettings.Length == 0) 
            {
                ArrayUserSettings = new string[] { "handsome", System.DateTime.Now.ToString() , "Microsoft Zira Desktop" };
            }
            // With this, you could also use different speakers. Just write them on the third line. and/or import the ArrayUserSettings[2] in something 
            if (ArrayUserSettings.Length >= 2) sophie.SelectVoice(ArrayUserSettings[2]);
            else sophie.SelectVoice("Microsoft Zira Desktop");

        }

        private void Speechreco_AudioRecognized(object sender, AudioLevelUpdatedEventArgs e)
        {
            progressBar1.Value = e.AudioLevel;
        }

        private void sophie_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            if (sophie.State == SynthesizerState.Speaking) sophie.SpeakAsyncCancelAll();
        }

        private SpeechRecognitionEngine CreateSpeechEngine(string preferredCulture)
        {
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (config.Culture.ToString() == preferredCulture)
                {
                    speechreco = new SpeechRecognitionEngine(config);
                    break;
                }
            }
            // if the desired culture is not found, then load default
            if (speechreco == null)
            {
                MessageBox.Show("The desired culture is not installed on this machine, the speech-engine will continue using "
                    + SpeechRecognitionEngine.InstalledRecognizers()[0].Culture.ToString() + " as the default culture.",
                    "Culture " + preferredCulture + " not found!");
                speechreco = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
            }
            return speechreco;
        }

        private void loadGrammar()
        {
            try
            {
                Choices texts = new Choices();
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\Grammar.txt");
                texts.Add(lines);
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                speechreco.LoadGrammar(wordsList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void loadCommands()
        {
            try
            {
                Choices texts = new Choices();
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\Commands.txt");
                texts.Add(lines);
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                speechreco.LoadGrammar(wordsList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SleepGrammarAndCommands()
        {
            try
            {
                Choices textz = new Choices();
                string[] linez = File.ReadAllLines(Environment.CurrentDirectory + "\\SleepCommands.txt");
                textz.Add(linez);
                Grammar wordsListz = new Grammar(new GrammarBuilder(textz));
                speechreco.LoadGrammar(wordsListz);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void UnloadAndLoadSleep()
        {
            speechreco.UnloadAllGrammars();
            SleepGrammarAndCommands();
        }
        private void UnloadAndLoadCommands()
        {
            speechreco.UnloadAllGrammars();
            loadCommands();
        }
        private void UnloadAndLoadGrammar()
        {
            speechreco.UnloadAllGrammars();
            loadGrammar();
        }



        private void Speechreco_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            /*
             * Could create a text document with only sophie
             *  Then when Sophie is mentioned, loadGrammarAndCommands
             * See how I use sleep. This could also be done to trigger Jarvis, but I like this better
             * Possibly have a new text file for tasks, such as call the Weather Function.
             * Should call Weather, until we close weather, close in itself should loadGrammar
             * 
             * Check IMDB ratings
             * Evt check movie times Adda kino (maa forandre hver gang)
             */
            OutputTextBox.Text = e.Result.Text;
            string speech = (e.Result.Text);
            switch (speech)
            {
                //greetings
                case "hello":
                case "hi":
                case "hello sophie":
                    System.DateTime timenow = System.DateTime.Now;
                    if (timenow.Hour >= 5 && timenow.Hour < 12) sophie.SpeakAsync("Good morning " + ArrayUserSettings[0]);
                    else if (timenow.Hour >= 12 && timenow.Hour < 18) sophie.SpeakAsync("Good afternoon " + ArrayUserSettings[0]);
                    else  sophie.SpeakAsync("Good Evening " + ArrayUserSettings[0]);
                    loadGrammar();
                   break;
                //attitudes
                case "sophie who is william":
                    sophie.SpeakAsync("He is a sexy guy from Sweden playing Football");
                    loadGrammar();
                    break;
                case "how are you":
                    sophie.SpeakAsync("i am happy as always " + ArrayUserSettings[0] + ", how about you");
                    loadGrammar();
                    break;
                // Time and Date
                case "time": //time
                    System.DateTime now = System.DateTime.Now;
                    string time = now.GetDateTimeFormats('t')[0];
                    sophie.SpeakAsync(time);
                    loadGrammar();
                    break;
                case "date": //date
                    string date = System.DateTime.Now.ToString("dd MMM", new System.Globalization.CultureInfo("en-US")) + "" + System.DateTime.Today.ToString(" yyyy");
                    sophie.SpeakAsync(date);
                    loadGrammar();
                    break;
                //Useful Facts
                case "internet status": //internet
                    bool network = NetworkInterface.GetIsNetworkAvailable();
                    if (network) sophie.SpeakAsync("You are Connected to the Internet");
                    else sophie.SpeakAsync("You are Disconnected from the Internet");
                    loadGrammar();
                    break;
                case "read me poetry":
                    string poem = getPoetry();
                    sophie.SpeakAsync(poem);
                    loadGrammar();
                    break;
                case "tell me a story":
                    string story = getShortStory();
                    sophie.SpeakAsync(story);
                    loadGrammar();
                    break;
                //ToDos
                case "open google":
                case "can you open google":
                    sophie.SpeakAsync("loading");
                    System.Diagnostics.Process.Start("http://www.google.com");
                    UnloadAndLoadCommands();
                    break;
                case "open facebook":
                case "can you open facebook":
                    sophie.SpeakAsync("loading");
                    System.Diagnostics.Process.Start("http://www.facebook.com");
                    UnloadAndLoadCommands();
                    break;
                case "sophie weather": // weather
                    sophie.SpeakAsync("Loading Weather");
                    UnloadAndLoadCommands();
                    //WeatherReader weather_read = new WeatherReader();
                    //weather_read.Show();
                    //weather_read.TopMost = true;
                    break;
                case "sophie write": // writer
                    sophie.SpeakAsync("Loading Writer");
                    UnloadAndLoadCommands();
                    //TextReader text_read = new TextReader();
                    //text_read.Show();
                    //text_read.TopMost = true;
                    break;
                case "sophie calculate": // calculator
                    sophie.SpeakAsync("Loading Weather");
                    UnloadAndLoadCommands();
                    //Calculator calculate = new Calculator();
                    //calculate.Show();
                    //calculate.TopMost = true;
                    break;
                // ShutDowns
                case "sophie sleep": //sleep
                    sophie.SpeakAsync("see you later, ali gator");
                    Thread.Sleep(2400);
                    UnloadAndLoadSleep();
                    break;
                case "sophie wake up": // wake up from sleep
                    sophie.SpeakAsync("What can I help you with?");
                    UnloadAndLoadGrammar();
                    break;
                case "sophie shutdown": //sleep
                    sophie.SpeakAsync("shutting down");
                    Application.Exit();
                    this.Close();
                    break;
                case "stop":
                case "sophie stop":
                    sophie.SpeakAsyncCancelAll();
                    UnloadAndLoadGrammar();
                    break;
                case "sophie who is eirin":
                    sophie.SpeakAsync("She is a pretty funny girl you matched on Tinder");
                    UnloadAndLoadGrammar();
                    break;
                case "should eirin go out with me":
                    sophie.SpeakAsync("Yes! She should. You are worth it, Master");
                    UnloadAndLoadGrammar();
                    break;
                case "close":
                    sophie.SpeakAsyncCancelAll();
                    Process[] AllProcesses = Process.GetProcesses();
                    foreach (var process in AllProcesses)
                    {
                        if (process.MainWindowTitle != "")
                        {
                            string s = process.ProcessName.ToLower();
                            if (s == "iexplore" || s == "iexplorer" || s == "chrome" || s == "firefox" || s == "microsoftedgecp") process.Kill();//currently does not work on my computer?
                        }
                    }
                    UnloadAndLoadGrammar();
                    break;
                case "sophie commands":
                    String lines = File.ReadAllText(Environment.CurrentDirectory + "\\Grammar.txt");
                    sophie.SpeakAsync("My Pleasure, all the commands are : " + lines);
                    Thread.Sleep(8000);
                    UnloadAndLoadGrammar();
                    break;
                // NEED TO ADD / Commands
                // Sophie has to be static, and user should be able to create new commands
                default:
                    UnloadAndLoadGrammar();
                    break;     
            }
        }

        private string getShortStory()  //TODO CURRENTLY ONLY 1 STORY
        {
            string LittleJohnny = ("A teacher asks her class : if there are 5 birds sitting on a fence and you shoot one of them, how many will it be left?    " +
    "She calls on little Johnny. He replies : None, they all fly away with the first gun shot.  " +
    "The teacher replies : The correct answer is 4, but I like your thinking. " +
    "Then, Little Johnny says : I have a question for You. There are three women sitting on a bench having ice cream   " +
    ": One is delicately licking the sides of the triple scoop of ice cream. " +
    "The second is gobbling down the top and sucking the cone. " +
    "The third is biting off the top of the ice cream.   " +
    " Which one is married?" +
    " The teacher, blushing a great deal, replied : " +
    "Well I suppose the one that's gobbled down the top and sucked the cone.  " +
    "To which Little Johnny replied : " +
    "The correct answer is the one with the wedding ring on, but I like your thinking.");
            return LittleJohnny;
        }

        private string getPoetry()  //TODO CURRENTLY ONLY 1 POEM
        {
            string AClearMidnight = ("Reading : A Clear Midnight   By Walt Whitman.     " +
    "This is thy hour O Soul : thy free flight, into the wordless. " +
    "Away from books. Away from art. The day erased : The lesson done. " +
    "Thee fully forth emerging : silent, gazing : pondering the themes  thou lovest best. " +
    "Night. Sleep. Death, and the stars");
            return AClearMidnight;
        }
    }
}
