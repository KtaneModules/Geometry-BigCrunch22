using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using KModkit;

public class GeometryScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
	public KMBombModule Module;
	
	public KMSelectable[] Numbers, OtherButtons;
	public SpriteRenderer Letters;
	public Sprite[] Shape;
	public TextMesh Design, Counter, NumberDisplay;
	public TextAsset MathTerms;
	public AudioSource MusicPlayer;
	public AudioClip[] SFX;
	
	decimal Wafer = 0;
	bool Interactable = false;
	string[] Alphabreak = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"}, Mechanon = {"∡", "⊥", "∥", "≅", "~", "Δ"};
	int[] ShapesAndNumbers = new int[3];
	Coroutine FastSpin;
	string InputNumber = "", Symbol = "", CorrectAnswer = "";
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved = false;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
        for (int a = 0; a < Numbers.Count(); a++)
        {
            int NumberPos = a;
            Numbers[NumberPos].OnInteract += delegate
            {
                PressNumber(NumberPos);
				return false;
            };
        }
		
		for (int a = 0; a < OtherButtons.Count(); a++)
        {
            int NumberPos = a;
            OtherButtons[NumberPos].OnInteract += delegate
            {
                PressOtherButton(NumberPos);
				return false;
            };
        }
	}
	
	void Start()
	{
		Module.OnActivate += GenerateAbsolutelyEverything;
	}
	
	void PressOtherButton(int Position)
	{
		OtherButtons[Position].AddInteractionPunch(0.25f);
		if (!ModuleSolved && Interactable)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			if (Position == 0)
			{
				if (InputNumber.ToCharArray().Count(c => c == '.') == 0 && InputNumber.Length > 0)
				{
					InputNumber += ".";
					NumberDisplay.text = InputNumber;
				}
			}
			
			else if (Position == 1)
			{
				if (InputNumber.Length > 0)
				{
					InputNumber = InputNumber.Remove(InputNumber.Length - 1);
					NumberDisplay.text = InputNumber;
					if (InputNumber.Length == 0)
					{
						FastSpin = StartCoroutine(SpinRealquick());
						Design.text = Symbol;
					}
				}
			}
			
			else
			{
				if (InputNumber.Length != 0 && ((InputNumber.Split('.').Length == 1) || (InputNumber.Split('.').Length == 2 && InputNumber.Split('.')[1].Length != 0)))
				{
					Interactable = false;
					StartCoroutine(ACheck());
				}
			}
		}
	}
	
	void PressNumber(int Position)
	{
		Numbers[Position].AddInteractionPunch(0.25f);
		if (!ModuleSolved && Interactable)
		{
			Audio.PlaySoundAtTransform(SFX[0].name, transform);
			if ((InputNumber.Split('.').Length == 1 && InputNumber.Length < 6) || (InputNumber.Split('.').Length == 2 && InputNumber.Split('.')[1].Length < 6))
			{
				if (InputNumber == "")
				{
					StopCoroutine(FastSpin);
					Counter.text = "";
					Letters.sprite = null;
					Design.text = "";
				}
				InputNumber = InputNumber == "0" ? Position.ToString() : InputNumber + Position.ToString();
				NumberDisplay.text = InputNumber;
				
			}
		}
	}
	
	IEnumerator ACheck()
	{
		yield return null;
		if (InputNumber == CorrectAnswer)
		{
			Debug.LogFormat("[Geometry #{0}] You sent {1}, which is  correct. Module solved.", moduleId, InputNumber);	
			int[] PreviousNumbers = {-1, -1};
			MusicPlayer.clip = SFX[4];
			NumberDisplay.text = "";
			MusicPlayer.Play();
			while (MusicPlayer.isPlaying)
			{
				int[] TempNumbers = {-1, -1};
				do
				{
					TempNumbers[0] = UnityEngine.Random.Range(0,7);
					TempNumbers[1] = UnityEngine.Random.Range(0,Mechanon.Length);
				}
				while (TempNumbers[0] == PreviousNumbers[0] || TempNumbers[1] == PreviousNumbers[1]);
				Letters.sprite = Shape[TempNumbers[0]];
				Design.text = Mechanon[TempNumbers[1]];
				yield return new WaitForSecondsRealtime(0.1f);
			}
			Letters.sprite = null;
			Design.text = "";
			Module.HandlePass();
			NumberDisplay.color = new Color(24f/255f, 60f/255f, 0f);
			NumberDisplay.text = "CORRECT";
			Audio.PlaySoundAtTransform(SFX[5].name, transform);
			ModuleSolved = true;
		}
		
		else
		{
			Debug.LogFormat("[Geometry #{0}] You sent {1}, which is not correct. Prepare for a strike.", moduleId, InputNumber);	
			NumberDisplay.color = new Color32(100, 0, 0, 255);
			for (int x = 0; x < 3; x++)
			{
				MusicPlayer.clip = SFX[1+x];
				MusicPlayer.Play();
				NumberDisplay.text = x % 2 == 0 ? "INCORRECT" : "";
				while (MusicPlayer.isPlaying)
				{
					yield return new WaitForSecondsRealtime(0.01f);
				}
			}
			Module.HandleStrike();
			NumberDisplay.text = InputNumber = "";
			NumberDisplay.color = Color.black;
			FastSpin = StartCoroutine(SpinRealquick());
			Design.text = Symbol;
			Interactable = true;
		}
	}
	
	void GenerateAbsolutelyEverything()
	{
		string Decamen = Bomb.GetSerialNumber();
		for (int y = 0; y < Decamen.Length; y++)
		{
			if (!Decamen[y].ToString().EqualsAny(Alphabreak))
			{
				Wafer += Int32.Parse(Decamen[y].ToString());
			}
		}
		Wafer *= Bomb.GetSerialNumberLetters().Count();
		Wafer += Bomb.IsIndicatorOn("BOB") ? 69 : 0;
		Debug.LogFormat("[Geometry #{0}] The initial number generated: {1}", moduleId, Wafer);	
		Symbol = Design.text = Mechanon[UnityEngine.Random.Range(0, Mechanon.Length)];
		switch (Design.text)
		{
			case "∡":
				Wafer *= 30M;
				break;
			case "⊥":
				Wafer /= 2M;
				break;
			case "∥":
				Wafer += Bomb.GetBatteryCount();
				break;
			case "≅":
				while (Wafer.ToString().Length != 1)
				{
					int Guide = 0;
					string Heckel = Wafer.ToString();
					for (int y = 0; y < Heckel.Length; y++)
					{
						Guide += Int32.Parse(Heckel[y].ToString());
					}
					Wafer = Guide;
				}
				break;
			case "~":
				Wafer += Bomb.GetSolvableModuleNames().Count();
				break;
			case "Δ":
				Wafer = Wafer * (2M/3M);
				break;
			default:
				break;
		}
		string[] Mechanona = Wafer.ToString().Split('.');
		if (Mechanona.Length != 1 && Mechanona[1].Length > 6)
		{
			Wafer = Math.Round(Wafer, 6, MidpointRounding.AwayFromZero);
		}
		Debug.LogFormat("[Geometry #{0}] The symbol given is {2}, which result in this new number: {1}", moduleId, Wafer, Symbol);
		string[] Shapes = {"square", "triangle", "rectangle", "trapezoid", "circle", "hexagon", "heptagon"}, Ordinal = {"1st", "2nd", "3rd"};
		for (int x = 0; x < 3; x++)
		{
			ShapesAndNumbers[x] = UnityEngine.Random.Range(0,7);
			switch (ShapesAndNumbers[x])
			{
				case 0:
					Wafer = Wafer * Wafer;
					break;
				case 1:
					Wafer = (Wafer * ((Bomb.GetPortCount() + Bomb.GetBatteryCount() + 1) * 2M)) / 2M;
					break;
				case 2:
					Wafer = Wafer * ((Bomb.GetPortCount() + Bomb.GetBatteryCount() + 1) * 2M);
					break;
				case 3:
					Wafer = ((Wafer + Bomb.GetSerialNumberNumbers().Last() + Bomb.GetBatteryHolderCount(1)) * ((Bomb.GetPortCount() + Bomb.GetBatteryCount() + 1) * 2M)) / 2M;
					break;
				case 4:
					Wafer = (Wafer * Wafer) * 3.14159265M;
					break;
				case 5:
					Wafer = ((3M * (Wafer * Wafer)) * 1.73205081M) / 2M;
					break;
				case 6:
					Wafer = ((7M / 4M) * (Wafer * Wafer)) * 2.07652140M;
					break;
				default:
					break;
			}
			string[] Canoen = Wafer.ToString().Split('.');
			if (Canoen.Length != 1 && Canoen[1].Length > 6)
			{
				Wafer = Math.Round(Wafer, 6, MidpointRounding.AwayFromZero);
			}
			Wafer %= 1000000;
			Debug.LogFormat("[Geometry #{0}] The {3} shapen given is {2}, which result in this new number: {1}", moduleId, Wafer, Shapes[ShapesAndNumbers[x]], Ordinal[x]);
		}
		Wafer = Wafer < 0 ? Wafer * -1 : Wafer;
		
		string ValidRules = "";
		if (Wafer <= 500000)
		{
			ValidRules += "1";
			Debug.LogFormat("[Geometry #{0}] The final number is less than 500000. The resulting number was: {1}", moduleId, Wafer);
		}
		
		List<string> AllModules = Bomb.GetSolvableModuleNames();
		string[] ValidWords = JsonConvert.DeserializeObject<string[]>(MathTerms.text);
		for (int a = 0; a < AllModules.Count(); a++)
		{
			for (int b = 0; b < AllModules[a].Split(' ').Length; b++)
			{
				for (int c = 0; c < ValidWords.Length; c++)
				{
					if (Regex.IsMatch(AllModules[a].Split(' ')[b].ToUpper(), ValidWords[c]))
					{
						ValidRules += "2";
						Debug.LogFormat("[Geometry #{0}] The module \"{1}\" contained a math term.", moduleId, AllModules[a]);
						goto Skip;
					}
				}
			}
		}
		Skip:
		
		if (Bomb.GetSerialNumberNumbers().Last().ToString().EqualsAny("2", "3", "5", "7"))
		{
			ValidRules += "3";
			Debug.LogFormat("[Geometry #{0}] The last digit of the serial number is a prime number, which was: {1}", moduleId, Bomb.GetSerialNumberNumbers().Last().ToString());
		}
		
		if (new[] {'M', 'A', 'T', 'H'}.Any(c => Bomb.GetSerialNumber().Contains(c)))
		{
			ValidRules += "4";
			Debug.LogFormat("[Geometry #{0}] The serial number {1} contains a letter from \"MATH\"", moduleId, Bomb.GetSerialNumber());
		}
		
		if (ValidRules.EqualsAny("234", "3", "34", "24"))
		{
			Wafer = Wafer / 4M;
			string[] Canoen = Wafer.ToString().Split('.');
			if (Canoen.Length != 1 && Canoen[1].Length > 6)
			{
				Wafer = Math.Round(Wafer, 6, MidpointRounding.AwayFromZero);
			}
			CorrectAnswer = Wafer.ToString();
			Debug.LogFormat("[Geometry #{0}] The \"P\" rule applied. The correct answer is {1}", moduleId, CorrectAnswer);
		}
		
		else if (ValidRules.EqualsAny("124", "4", "14", "13"))
		{
			CorrectAnswer = Wafer.ToString().Split('.')[0];
			Debug.LogFormat("[Geometry #{0}] The \"W\" rule applied. The correct answer is {1}", moduleId, CorrectAnswer);
		}
		
		else if (ValidRules.EqualsAny("134", "1", "23", "12"))
		{
			string[] Canoen = Wafer.ToString().Split('.');
			if (Canoen.Length == 1)
			{
				CorrectAnswer = "0";
			}
			else
			{
				CorrectAnswer = Canoen[1].TrimStart(new Char[] { '0' } );
				CorrectAnswer = CorrectAnswer == "" ? "0" : CorrectAnswer;
			}
			Debug.LogFormat("[Geometry #{0}] The \"I\" rule applied. The correct answer is {1}", moduleId, CorrectAnswer);
		}
		
		else
		{
			CorrectAnswer = Wafer.ToString();
			Debug.LogFormat("[Geometry #{0}] The \"N\" rule applied. The correct answer is {1}", moduleId, CorrectAnswer);
		}
		FastSpin = StartCoroutine(SpinRealquick());
		Interactable = true;
	}
	
	IEnumerator SpinRealquick()
	{
		while (true)
		{
			for (int x = 0; x < 3; x++)
			{
				Counter.text = (x+1).ToString();
				Letters.sprite = Shape[ShapesAndNumbers[x]];
				yield return new WaitForSecondsRealtime(0.2f);
			}
			Counter.text = "";
			Letters.sprite = null;
			yield return new WaitForSecondsRealtime(0.4f);
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit a number on the module, use the command !{0} submit [VALID NUMBER] (Example: !{0} submit 123.456789)";
    #pragma warning restore 414
	
	string[] ValidNumbers = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
	
    IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Parameter length invalid. Command ignored.";
				yield break;
			}
			
			if (!Interactable)
			{
				yield return "sendtochaterror You are unable to interact with the module currently. Command ignored.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				if (!new [] {parameters[1][x].ToString()}.Any(c => ValidNumbers.Contains(c)) && parameters[1][x].ToString() != ".")
				{
					yield return "sendtochaterror Your number contains an invalid character. Command ignored.";
					yield break;
				}
			}
			
			if (parameters[1].ToCharArray().Count(c => c == '.') > 1)
			{
				yield return "sendtochaterror " + parameters[1].ToCharArray().Count(c => c == '.').ToString() + " decimal points are invalid. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Split('.').Length == 2 && parameters[1].Split('.')[0].Length == 0)
			{
				yield return "sendtochaterror There was no number before the decimal point. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Split('.').Length == 2 && parameters[1].Split('.')[1].Length == 0)
			{
				yield return "sendtochaterror There was no number after the decimal point. I do not allow it. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Split('.')[0].Length > 1 && (parameters[1].Split('.')[0][0].ToString() == "0" && parameters[1].Split('.')[0][1].ToString() == "0"))
			{
				yield return "sendtochaterror The integer had 2 or more leading zeroes. I do not allow it. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Split('.')[0].Length > 6)
			{
				yield return "sendtochaterror The integer had 7 or more digits. Command ignored.";
				yield break;
			}
			
			if (parameters[1].Split('.').Length == 2 && parameters[1].Split('.')[1].Length > 6)
			{
				yield return "sendtochaterror The decimal had 7 or more digits. Command ignored.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Split('.').Length; x++)
			{
				for (int y = 0; y < parameters[1].Split('.')[x].Length; y++)
				{
					Numbers[Int32.Parse(parameters[1].Split('.')[x][y].ToString())].OnInteract();
					yield return new WaitForSecondsRealtime(0.1f);
				}
				
				if (x == 0 && parameters[1].Split('.').Length == 2)
				{
					OtherButtons[0].OnInteract();
					yield return new WaitForSecondsRealtime(0.1f);
				}
			}
			
			yield return "strike";
			yield return "solve";
			OtherButtons[2].OnInteract();
		}
	}
}
