using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System.Reflection;

public class OneDimensionalMaze : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable[] Buttons;
   public Material[] color;
   public KMSelectable Ball;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   static int onedCounter = 1;
   int onedID;

   private readonly List<int> Position = new List<int> { 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 0 };
   int CorrectRow;
   int CurrentPosition;
   int StartingPosition;

   private readonly List<float> AnimationTiming = new List<float> { 0.26666f, 0.13333f, 0.06666f, 0.03333f, .01667f, .008335f };

   string SN = "";

   private bool striking;

   void Awake () {
      GetComponent<KMBombModule>().OnActivate += Activate;
      onedID = onedCounter++;
      moduleId = moduleIdCounter++;
      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { ButtonPress(Button); return false; };
      }
      Ball.OnInteract += delegate () { PressBall(); return false; };
   }

   void Start () {
      SN = Bomb.GetSerialNumber();
      CorrectRow = (int) Char.GetNumericValue(SN[5]);
      StartingPosition = UnityEngine.Random.Range(0, Position.Count());
      CurrentPosition = StartingPosition;
      Ball.GetComponent<MeshRenderer>().material = color[Position[StartingPosition]];
      Debug.LogFormat("[1D Maze #{0}] You started at cell ({1}, {2}). (0, 0) is the top left, and it is listed column then row.", moduleId, CurrentPosition % 10, CurrentPosition / 10);
      Debug.LogFormat("[1D Maze #{0}] The desired cell is any in row {1}.", moduleId, Bomb.GetSerialNumberNumbers().Last());
   }

   void ButtonPress (KMSelectable Button) {
      if (moduleSolved) {
         Button.AddInteractionPunch();
         return;
      }
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
      if (Button == Buttons[1]) {
         CurrentPosition++;
         CurrentPosition = ExMath.Mod(CurrentPosition, 80);
      }
      else if (Button == Buttons[0]) {
         CurrentPosition--;
         CurrentPosition = ExMath.Mod(CurrentPosition, 80); //It says these are throwing errors, but they aren't. I don't know why.
      }
      Ball.GetComponent<MeshRenderer>().material = color[Position[CurrentPosition]];
   }

   void PressBall () {
      if (moduleSolved) {
         return;
      }
      Ball.AddInteractionPunch();
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Ball.transform);
      for (int i = 0; i < 10; i++) {
         if (CorrectRow == i) {
            if (CurrentPosition % 10 == i) {
               GetComponent<KMBombModule>().HandlePass();
               Debug.LogFormat("[1D Maze #{0}] You submitted at cell ({1}, {2}). Module disarmed.", moduleId, CurrentPosition % 10, CurrentPosition / 10);
               Audio.PlaySoundAtTransform("Farfare", transform);
               moduleSolved = true;
               Ball.GetComponent<MeshRenderer>().material = color[2];
               StartCoroutine(SolveAnimation());
            }
            else {
               GetComponent<KMBombModule>().HandleStrike();
               Debug.LogFormat("[1D Maze #{0}] You submitted at cell ({1}, {2}). Strike, poopyhead.", moduleId, CurrentPosition % 10, CurrentPosition / 10);
               if (striking) {
                  StopAllCoroutines();
               }
               StartCoroutine(StrikeHappened());
            }
         }
      }
   }
   void Activate () {
      if (GetMissionID() == "mod_TheFortySevenButAwesome_The 47") {
         return;
      }
      if (onedID == 1) {
         Audio.PlaySoundAtTransform("Activate", transform);
      }
      StartCoroutine(ResetID());
   }

   IEnumerator ResetID () {
      yield return new WaitForSeconds(1f);
      onedCounter = 1;
   }

   IEnumerator SolveAnimation () {
      for (int j = 0; j < 6; j++) {
         for (int k = 0; k < 6 - j; k++) {
            Ball.GetComponent<MeshRenderer>().material = color[0];
            yield return new WaitForSeconds(AnimationTiming[j]);
            Ball.GetComponent<MeshRenderer>().material = color[1];
            yield return new WaitForSeconds(AnimationTiming[j]);
         }
      }
      Ball.GetComponent<MeshRenderer>().material = color[2];
   }

   IEnumerator StrikeHappened () {
      striking = true;
      Ball.GetComponent<MeshRenderer>().material = color[3];
      yield return new WaitForSeconds(1f);
      Ball.GetComponent<MeshRenderer>().material = color[Position[CurrentPosition]];
      striking = false;
   }

   private string GetMissionID () {
      try {
         Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
         Type type = gameplayState.GetType();
         FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
         return fieldMission.GetValue(gameplayState).ToString();
      }

      catch (NullReferenceException) {
         return "undefined";
      }
   }

   //twitch plays
#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"!{0} [press] up/down [#] (Press the up or down arrow (optionally '#' times)) [slow] (slows each button press down) | !{0} submit (Presses the LED)";
#pragma warning restore 414
   IEnumerator ProcessTwitchCommand (string command) {
      Regex regex = new Regex(@"^(?:(?:press )?(up|down) (\d+)( slow)?|submit|(?:press )?up|(?:press )?down)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
      Match match = regex.Match(command);

      if (match.Success) {
         if (match.Groups[0].Value.Equals("submit", StringComparison.OrdinalIgnoreCase)) {
            yield return null;
            Ball.OnInteract();
            yield break;
         }

         string direction = match.Groups[1].Value;
         int temp;

         if (int.TryParse(match.Groups[2].Value, out temp)) {
            if (temp < 1 || temp > 79) {
               yield return "sendtochaterror The specified number of times to press the " + direction + " arrow " + temp + " times is out of the acceptable range of 1-79!";
               yield break;
            }

            // Default speed
            float delay = 0.1f;

            // If slow is mentioned, slow it down.
            if (match.Groups[3].Success) {
               delay = 0.8f;
            }

            // Determine which button to press based on the command.
            int buttonIndex = direction == "up" ? 0 : 1;

            for (int i = 0; i < temp; i++) {
               yield return null;
               Buttons[buttonIndex].OnInteract();
               yield return new WaitForSeconds(delay);
            }
         }
         else {
            // Since we've simplified the pattern, we can now check the entire matched command directly.
            switch (match.Value.ToLower()) {
               case "press up":
               case "up":
                  yield return null;
                  Buttons[0].OnInteract();
                  yield break;
               case "press down":
               case "down":
                  yield return null;
                  Buttons[1].OnInteract();
                  yield break;
            }
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      if (CurrentPosition % 10 != CorrectRow) {
         int Button = 0;
         int Dif = CorrectRow % 10 - CurrentPosition % 10;
         if ((Dif < 5 && Dif > 0) || (Dif < -5)) {
            Button = 1;
         }
         if (Dif == 5 || Dif == -5) {
            Button = UnityEngine.Random.Range(0, 2);
         }
         while (CurrentPosition % 10 != CorrectRow) {
            Buttons[Button].OnInteract();
            yield return new WaitForSeconds(0.1f);
         }
      }
      Ball.OnInteract();
   }
}
