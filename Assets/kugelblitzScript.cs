using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class kugelblitzScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo BombInfo;
	public GameObject[] Orbs;
	public GameObject[] Glow;
	public KMSelectable Sphere;
	public GameObject Shiny;
	public KMBombModule Module;
	public KMColorblindMode CBM;
	private KMAudio.KMAudioRef sound;

	private const int ADDED_STAGES = 0;
	private const bool PERFORM_AUTO_SOLVE = false;
	private const float STAGE_DELAY = 5f;
	private bool forcedSolve = false;
	public static string[] ignoredModules = null;
	private bool ready = false;
	private bool struck = false;
	private bool cruel = false;
	private bool secseq = false;
	private bool holdA = false;

	private float[] r = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] x = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] y = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] z = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] r2 = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] vx = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] vy = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] vz = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float[] vmod = { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
	private float vmod2 = 1f;
	private float[] cr = { 1f, 1f,   1f, 0f, 0f,    0.25f, 0.75f };
	private float[] cg = { 0f, 0.5f, 1f, 1f, 0.75f, 0f,    0f    };
	private float[] cb = { 0f, 0f,   0f, 0f, 1f,    1f,    1f    };
	private float[] nr = { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
	private float[] ng = { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
	private float[] nb = { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
	private float[] statuslight = { 0f, 1f, 0f };
	private int[] Display;
	private int targetNum;
	private int direction;
	private int[,] grid = {	{ 5, 1, 3, 6, 4, 0, 2},
							{ 1, 2, 6, 4, 0, 5, 3},
							{ 4, 0, 5, 1, 3, 2, 6},
							{ 3, 5, 2, 0, 1, 6, 4},
							{ 0, 3, 4, 2, 6, 1, 5},
							{ 6, 4, 0, 5, 2, 3, 1},
							{ 2, 6, 1, 3, 5, 4, 0} };
	private int[] pivot = { 0, 0, 0 };
	private int[] stored = { 0, 0, 0, 0, 0, 0, 0 };
	private int[] storedb = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	private int[] xtransf = { 0, 1, 1, 1, 0, -1, -1, -1 };
	private int[] ytransf = { -1, -1, 0, 1, 1, 1, 0, -1 };
	private int rotation = 3;
	private string userSequence = "";
	private int[] cruelInteract = { };
	private int[] cruelSplit = { 0 };
	private int cruelI = 0;
	private int cruelI2 = 0;
	private int cruelPos = 0;
	private long bigStore = 0L;
	private string controlSequence = "";
	private string cruelSequence = "[(";
	private int t = 0;
	private int solved = 0;
	private int memcount = 0;
	private int cbcount = 0;
	private float[] basecol = { 0.03125f, 0f, 0.0625f };
	private float scale = 0.1f;

	private bool solve = false;
	private bool hold = false;
	private int correctcount;

	private bool colorblind;
	public static int modID = 1;
	public int currentModID;

	//⊢∸−⊣∙⟛⫣⊩‖|

	// Use this for initialization
	void Awake () {

		currentModID = modID++;
		colorblind = CBM.ColorblindModeActive;

		if (ignoredModules == null)
			ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("Kugelblitz", new string[]{
				"Forget Me Not",     
				"Forget Everything", 
				"Turn The Key",      
				"Souvenir",         
				"The Time Keeper", 
				"Simon's Stages", 
				"Forget It Not",
				"Forget This",
				"Forget Them All",
				"Divided Squares",
				"Übermodule",
				"Encryption Bingo",
				"Organisation",
				"Ultimate Custom Night",
				"RPS Judging",
				"Cookie Jar",
				"Brainf---",
				"Kugelblitz" //for the sake of safety
			});

		Sphere.OnInteract += delegate ()
		{
			if (solved != Display.Length)
			{
				Module.HandleStrike(); Sphere.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0f, 0f); basecol = new float[] { 0.5f, 0f, 0f }; hold = false;
			}
			else
			{
				if (!solve)
				{
					if (!cruel)
					{
						Sphere.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
						Shiny.GetComponent<Light>().color = new Color(1f, 1f, 1f, 0.5f);
						basecol = new float[] { 1f, 1f, 1f };
					}
					else if (!secseq) {
						Sphere.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
						Shiny.GetComponent<Light>().color = new Color(1f, 0.5f, 0f, 0.5f);
						basecol = new float[] { 1f, 0.5f, 0f };
					}
					else
					{
						if (!holdA)
						{
							Sphere.GetComponent<MeshRenderer>().material.color = new Color(0f, 0.5f, 1f);
							Shiny.GetComponent<Light>().color = new Color(0f, 0.5f, 1f, 0.5f);
							basecol = new float[] { 0f, 0.5f, 1f };
						}
						else
						{
							Sphere.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
							Shiny.GetComponent<Light>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
							basecol = new float[] { 0.5f, 0.5f, 0.5f };
						}
					}
				}
				hold = true; 
				userSequence += '[';
				if (userSequence == "[")
				{
					sound = Audio.PlaySoundAtTransformWithRef("VoidSucc", Module.transform);
				}
				if (!secseq)
				{
					cruelInteract = cruelInteract.Concat(new int[] { t }).ToArray();
					cruelSplit[cruelI]++;
				}
			}
			return false;
		};
		Sphere.OnInteractEnded += delegate ()
		{
			if (solved == Display.Length && hold)
			{
				hold = false;
				userSequence += ']';
				if (!secseq)
				{
					cruelInteract = cruelInteract.Concat(new int[] { t }).ToArray();
					cruelSplit[cruelI]++;
				}
				if (holdA)
				{
					/*Sphere.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
					Shiny.GetComponent<Light>().color = new Color(1f, 0.5f, 0f, 0.5f);
					basecol = new float[] { 1f, 0.5f, 0f };*/
				}
			}
		};

		GetComponent<KMBombModule>().OnActivate += ActivateModule;

		for (int i = 0; i < 7; i++)
		{
			r[i] = Rnd.Range(0.055f, 0.09f);
			x[i] = Rnd.Range(-1f, 1f); y[i] = Rnd.Range(-1f, 1f); z[i] = Rnd.Range(-1f, 1f);
			r2[i] = Mathf.Pow(Mathf.Pow(x[i], 2f) + Mathf.Pow(y[i], 2f) + Mathf.Pow(z[i], 2f), 0.5f);
			x[i] = x[i] * r[i] / r2[i]; y[i] = y[i] * r[i] / r2[i]; z[i] = z[i] * r[i] / r2[i];
			vx[i] = Rnd.Range(-.1f, .1f); vy[i] = Rnd.Range(-.1f, .1f); vz[i] = Rnd.Range(-.1f, .1f);
			Orbs[i].transform.localPosition = new Vector3(x[i], y[i], z[i]);
			vmod[i] = 0.0000125f / Mathf.Pow(r[i], 2f);
		}
		Sphere.GetComponentInChildren<TextMesh>().text = "";
	}

	private void Start()
	{
		StartCoroutine(StartingKugel());
	}

	private void ActivateModule()
	{
		int count = BombInfo.GetSolvableModuleNames().Count(a => !ignoredModules.Contains(a)) + ADDED_STAGES;
		string[] cardinals = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest"};
		string[] rotate = { "clockwise", "counterclockwise" };
		Debug.LogFormat("[Kugelblitz #{0}] found {1} solvable modules.", currentModID, count);
		if (count == 0) { Module.HandlePass(); forcedSolve = true; } //Prevent deadlock
		else
		{
			Display = new int[count];
			targetNum = Rnd.Range(0, 98);
			int previous = 0;
			for (int i = 0; i < count - 1; i++)
			{
				Display[i] = Rnd.Range(0, 128);
				previous = Display[i] ^ previous;
			}
			Display[count - 1] = targetNum ^ previous;
			direction = Rnd.Range(0, 8);
			pivot[0] = (targetNum / 7) % 7;
			pivot[1] = targetNum % 7;
			pivot[2] = direction % 8;
			if (targetNum / 49 == 1)
				rotation *= -1;
			Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers are {1}. This will result in {2}.", currentModID, Display.Join(", "), targetNum);
			Debug.LogFormat("[Kugelblitz #{0}] Starting at ({1}, {2}) in direction {3}, starting with a rotational modifier of 135 degrees {4}.", currentModID, pivot[0], pivot[1], cardinals[direction], rotate[targetNum / 49]);
			for (int i = 0; i < 13; i++)
			{
				for (int j = 0; j <= i; j++)
				{
					//Debug.LogFormat("[Kugelblitz #{0}] ({1}, {2}), {3}.", currentModID, pivot[0], pivot[1], grid[pivot[1], pivot[0]]);
					pivot[2] = (pivot[2] + 16) % 8;
					storedb[i] += grid[pivot[1], pivot[0]];
					pivot[0] += xtransf[pivot[2]];
					pivot[1] += ytransf[pivot[2]];
					if (!(0 <= pivot[0] && pivot[0] < 7))
					{
						pivot[1] = 6 - pivot[1];
						pivot[0] = (pivot[0] + 7) % 7;
						rotation *= -1;
						pivot[2] = 4 - pivot[2];

					}
					if (!(0 <= pivot[1] && pivot[1] < 7))
					{
						pivot[0] = 6 - pivot[0];
						pivot[1] = (pivot[1] + 7) % 7;
						rotation *= -1;
						pivot[2] = -pivot[2];
					}
				}
				storedb[i] %= 7;
				if (i < 7)
					stored[i] = storedb[i];
				pivot[2] = pivot[2] + rotation;
			}
			for (int i = 0; i < 7; i++)
			{
				bigStore = bigStore * 7 + stored[i];
			}
			Debug.LogFormat("[Kugelblitz #{0}] Calculated values are {1}.", currentModID, stored.Join(", "));
			if (bigStore < 524288)
				bigStore += 524288;
			int j2 = 524288;
			int k = 0;
			bool h = false;
			for (int i = 0; i < 20; i++)
			{
				if ((bigStore / j2) % 2 == 1)
				{
					if (k >= 3)
					{
						k = 0;
						controlSequence += '∙';
						if (k > -1)
							k = -1;
						else
							k--;
					}
					else
					{
						if (h)
							controlSequence += ']';
						else
							controlSequence += '[';
						h = !h;
						if (k < 1)
							k = 1;
						else
							k++;
					}
				}
				else
				{
					if (k <= -2 || (k <= -1 && !h))
					{
						k = 0;
						if (h)
						{
							controlSequence += ']';
						}
						else
							controlSequence += '[';
						h = !h;
						if (k < 1)
							k = 1;
						else
							k++;
					}
					else
					{
						controlSequence += '∙';
						if (k > -1)
							k = -1;
						else
							k--;
					}
				}
				j2 /= 2;
			}
			if (h)
				controlSequence += "]∙∙";
			else
			{
				if (controlSequence[controlSequence.Length - 1] != '∙' || controlSequence[controlSequence.Length - 2] != '∙')
				{
					while (controlSequence[controlSequence.Length - 1] != '∙' || controlSequence[controlSequence.Length - 2] != '∙')
						controlSequence += '∙';
				}
			}
			Debug.LogFormat("[Kugelblitz #{0}] The calculated sequence is {1}, which translates to {2}.", currentModID, controlSequence.Replace("]∙∙", "]").Replace("[", "i").Replace("]", "i").Replace("∙", "p"), controlSequence.Replace("]∙∙", "]").Replace("[∙∙]", "[∸−∸]").Replace("[∙", "[∸").Replace("∙]", "∸]").Replace("[][]", "‖").Replace("[][", "⊩").Replace("][]", "\u2AE3").Replace("][", "\u27DB").Replace("[]", "|").Replace("[", "⊢").Replace("]", "⊣"));
			
			Debug.LogFormat("[Kugelblitz #{0}] Cruel values are {1}.", currentModID, storedb.Join(", "));
			bigStore = 0;
			for (int i = 0; i < 13; i++)
			{
				bigStore = bigStore * 7 + storedb[i];
			}
			long j3 = 10460353203L;
			int ka = 1;
			int kb = 1;
			bool ha = true;
			bool hb = true;
			for (int i = 0; i < 22; i++)
			{
				if ((bigStore / j3) % 3L == 0L)
				{
					if (!((ka <= -2 || (ka <= -1 && !ha)) || (kb <= -2 || (kb <= -1 && !hb))))
					{
						cruelSequence += '∙';
						if (ka > -1)
							ka = -1;
						else
							ka--;
						if (kb > -1)
							kb = -1;
						else
							kb--;
					}
					else
					{
						if (ka <= -2 || (ka <= -1 && !ha))
						{
							if (ha)
								cruelSequence += ')';
							else
								cruelSequence += '(';
							ha = !ha;
							ka = 1;
						}
						else if (kb <= -2 || (kb <= -1 && !hb))
						{
							if (hb)
								cruelSequence += ']';
							else
								cruelSequence += '[';
							hb = !hb;
							kb = 1;
						}
						else
							cruelSequence += '-';
					}
				}
				else if ((bigStore / j3) % 3L == 1L)
				{
					if (ka >= 3)
					{
						if (!(kb <= -2 || (kb <= -1 && !hb)))
						{
							cruelSequence += '∙';
							ka = -1;
							if (kb > -1)
								kb = -1;
							else
								kb--;
						}
						else if (kb <= -2 || (kb <= -1 && !hb))
						{
							if (hb)
								cruelSequence += ']';
							else
								cruelSequence += '[';
							hb = !hb;
							kb = 1;
						}
						else
							cruelSequence += '∙';
					}
					else
					{
						if (ha)
							cruelSequence += ')';
						else
							cruelSequence += '(';
						ha = !ha;
						ka++;
						if (ka == 0)
							ka++;
					}
				}
				else
				{
					if (kb >= 3)
					{
						if (!(ka <= -2 || (ka <= -1 && !ha)))
						{
							cruelSequence += '∙';
							if (ka > -1)
								ka = -1;
							else
								ka--;
							kb = -1;
						}
						else if (ka <= -2 || (ka <= -1 && !ha))
						{
							if (ha)
								cruelSequence += ')';
							else
								cruelSequence += '(';
							ha = !ha;
							ka = 1;
						}
						else
							cruelSequence += '∙';
					}
					else
					{
						if (hb)
							cruelSequence += ']';
						else
							cruelSequence += '[';
						hb = !hb;
						kb++;
						if (kb == 0)
							kb++;
					}
				}
				j3 /= 3L;
			}
			cruelSequence = cruelSequence.Replace("-", "");
			if (ha && hb)
				cruelSequence += "])∙∙";
			else if (ha)
				cruelSequence += ")∙∙";
			else if (hb)
				cruelSequence += "]∙∙";
			else
			{
				if (cruelSequence[cruelSequence.Length - 1] != '∙' || cruelSequence[cruelSequence.Length - 2] != '∙')
				{
					while (cruelSequence[cruelSequence.Length - 1] != '∙' || cruelSequence[cruelSequence.Length - 2] != '∙')
						cruelSequence += '∙';
				}
			}
			Debug.LogFormat("[Kugelblitz #{0}] The cruel sequence is {1} or {2}.", currentModID, cruelSequence.Replace("-", "").Replace("]∙∙", "]").Replace(")∙∙", ")").Replace("(", "a").Replace(")", "a").Replace("[", "b").Replace("]", "b").Replace("∙", "p"), cruelSequence.Replace("-", "").Replace("]∙∙", "]").Replace(")∙∙", ")"));
			ready = true;
		}
	}
		
	// Update is called once per frame
	void FixedUpdate () {
		if (!forcedSolve)
		{
			cbcount %= 7;
			t++;
			t %= 150;
			if (!solve)
			{
				if (t == 0)
				{
					if (ready && (solved == Display.Length))
					{
						for (int i = 0; i < 7; i++)
						{
							if (!struck)
							{
								nr[i] = 0.03125f; ng[i] = 0f; nb[i] = 0.0625f;
							}
							else
							{
								if (6 - i < correctcount)
								{
									nr[i] = 1f; ng[i] = 1f; nb[i] = 1f;
								}
								else
								{
									nr[i] = 0.03125f; ng[i] = 0f; nb[i] = 0.0625f;
								}
							}
						}
						Audio.PlaySoundAtTransform("Pulse", Module.transform);
					}
					if (userSequence != "") { userSequence += '∙'; if (!secseq) { cruelI++; cruelSplit = cruelSplit.Concat(new int[] { 0 }).ToArray(); } }
					if (userSequence.Length > 2 && !hold && !holdA)
					{
						if (userSequence[userSequence.Length - 2] == '∙')
						{
							if (userSequence != controlSequence || cruel)
							{
								if (!cruel || secseq)
								{
									Sphere.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0f, 0f);
									basecol = new float[] { 0.5f, 0f, 0f };
									if (!cruel)
									{
										Module.HandleStrike();
										Debug.LogFormat("[Kugelblitz #{0}] You submitted {1}, but I expected {2}.", currentModID, userSequence.Replace("]∙∙", "]").Replace("[∙∙]", "[∸−∸]").Replace("[∙", "[∸").Replace("∙]", "∸]").Replace("[][]", "‖").Replace("[][", "⊩").Replace("][]", "\u2AE3").Replace("][", "\u27DB").Replace("[]", "|").Replace("[", "⊢").Replace("]", "⊣"), controlSequence.Replace("]∙∙", "]").Replace("[∙∙]", "[∸−∸]").Replace("[∙", "[∸").Replace("∙]", "∸]").Replace("[][]", "‖").Replace("[][", "⊩").Replace("][]", "\u2AE3").Replace("][", "\u27DB").Replace("[]", "|").Replace("[", "⊢").Replace("]", "⊣"));
										struck = true;
									}
									else
									{
										if (userSequence == controlSequence)
										{
											StartCoroutine(SolvingKugel());
											goto CruelBreak;
										}
										else
										{
											struck = true;
											Debug.LogFormat("[Kugelblitz #{0}] You submitted {1}, but I expected {2}.", currentModID, userSequence.Replace("]∙∙", "]").Replace(")∙∙", ")"), cruelSequence.Replace("]∙∙", "]").Replace(")∙∙", ")"));
											Module.HandleStrike();
										}
									}
									secseq = false;
									cruelInteract = new int[] { };
									cruelI = 0;
									cruelSplit = new int[] { 0 };
									correctcount = 0;
									for (int i = 0; i < userSequence.Length && i < controlSequence.Length; i++)
									{
										correctcount = (i / 3);
										if (userSequence[i] != controlSequence[i])
										{
											goto loopbreak;
										}
									}
								}
								else
								{
									Debug.LogFormat("[Kugelblitz #{0}] Recorded the following T values (0-150): {1}, split in groups of: {2}.", currentModID, cruelInteract.Join(", "), cruelSplit.Join(", "));
									secseq = true;
								}
								cruelI = 0;
								cruelI2 = 0;
								cruelPos = 0;
								loopbreak:
								userSequence = "";
								sound.StopSound();
								sound = null;
							}
							else
							{
								statuslight = new float[] { 1f, 1f, 1f };
								StartCoroutine(SolvingKugel());
							}
						}
					}
					else if (userSequence.Length > 3 && hold && userSequence[userSequence.Length - 2] == '∙' && userSequence[userSequence.Length - 3] == '∙')
					{
						cruelInteract = new int[] { };
						cruelI = 0;
						cruelSplit = new int[] { 0 };
						cruel = !cruel;
						userSequence = "";
						sound.StopSound();
						sound = null;
						hold = false;
					}
				}
				if (secseq && cruelPos < cruelInteract.Length)
				{
					if (cruel && t == cruelInteract[cruelPos] && userSequence != "")
					{
						if (cruelSplit[cruelI] != cruelI2)
						{
							if (!holdA)
							{
								userSequence += '(';
								if (hold)
								{
									Sphere.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
									Shiny.GetComponent<Light>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
									basecol = new float[] { 0.5f, 0.5f, 0.5f };
								}
								else
								{
									Sphere.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
									Shiny.GetComponent<Light>().color = new Color(1f, 0.5f, 0f, 0.5f);
									basecol = new float[] { 1f, 0.5f, 0f };
								}
							}
							else
							{
								userSequence += ')';
								/*if (hold)
								{
									Sphere.GetComponent<MeshRenderer>().material.color = new Color(0f, 0.5f, 1f);
									Shiny.GetComponent<Light>().color = new Color(0f, 0.5f, 1f, 0.5f);
									basecol = new float[] { 0f, 0.5f, 1f };
								}*/
							}
							holdA = !holdA;
							cruelPos++;
							cruelI2++;
							if (cruelSplit[cruelI] == cruelI2)
							{
								cruelI2 = 0;
								cruelI++;
								//Debug.Log("waiting time");
							}
						}
						else
						{
							cruelI2 = 0;
							cruelI++;
							//Debug.Log("waiting time");
						}
						//Debug.Log(cruelI + ", " + cruelI2 + ", " + cruelPos + ", " + cruelSplit.Join("-"));
					}
				}
				if (!hold && !holdA)
				{
					if (!cruel)
					{
						basecol[0] = (0.03125f + basecol[0] * 63f) / 64f; basecol[1] = (basecol[1] * 63f) / 64f; basecol[2] = (0.0625f + basecol[2] * 63f) / 64f;
					}
					else
					{
						if (!secseq)
						{
							basecol[0] = (1f + basecol[0] * 63f) / 64f; basecol[1] = (0.9375f + basecol[1] * 63f) / 64f; basecol[2] = (0.875f + basecol[2] * 63f) / 64f;
						}
						else
						{
							basecol[0] = (0.875f + basecol[0] * 63f) / 64f; basecol[1] = (0.9375f + basecol[1] * 63f) / 64f; basecol[2] = (1f + basecol[2] * 63f) / 64f;
						}
					}
					Sphere.GetComponent<MeshRenderer>().material.color = new Color(basecol[0], basecol[1], basecol[2]);
					Shiny.GetComponent<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
				}
				else if (secseq)
				{
					if (hold || holdA)
					{
						if (!hold)
						{
							basecol[0] = (1f + basecol[0] * 63f) / 64f; basecol[1] = (0.5f + basecol[1] * 63f) / 64f; basecol[2] = (0f + basecol[2] * 63f) / 64f;
						}
						if (!holdA)
						{
							basecol[0] = (0f + basecol[0] * 63f) / 64f; basecol[1] = (0.5f + basecol[1] * 63f) / 64f; basecol[2] = (1f + basecol[2] * 63f) / 64f;
						}
						Sphere.GetComponent<MeshRenderer>().material.color = new Color(basecol[0], basecol[1], basecol[2]);
						Shiny.GetComponent<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
					}
				}
				solved = BombInfo.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count();
				if (memcount != solved) { memcount = solved; StartCoroutine(KugelNextStage()); }
				int j = 1;

				string col = "?ROYGBIV";
				string bri = "-+";
				for (int i = 0; i < 7; i++)
				{
					r2[i] = Mathf.Pow(Mathf.Pow(x[i] + vx[i], 2f) + Mathf.Pow(y[i] + vy[i], 2f) + Mathf.Pow(z[i] + vz[i], 2f), 0.5f);
					vx[i] = (x[i] + vx[i]) * r[i] / r2[i]; vy[i] = (y[i] + vy[i]) * r[i] / r2[i]; vz[i] = (z[i] + vz[i]) * r[i] / r2[i];
					vx[i] = vx[i] - x[i]; vy[i] = vy[i] - y[i]; vz[i] = vz[i] - z[i];
					r2[i] = Mathf.Pow(Mathf.Pow(vx[i], 2f) + Mathf.Pow(vy[i], 2f) + Mathf.Pow(vz[i], 2f), 0.5f);
					vx[i] = vx[i] * vmod[i] * vmod2 / r2[i]; vy[i] = vy[i] * vmod[i] * vmod2 / r2[i]; vz[i] = vz[i] * vmod[i] * vmod2 / r2[i];
					x[i] = x[i] + vx[i]; y[i] = y[i] + vy[i]; z[i] = z[i] + vz[i];
					r2[i] = Mathf.Pow(Mathf.Pow(x[i], 2f) + Mathf.Pow(y[i], 2f) + Mathf.Pow(z[i], 2f), 0.5f);
					x[i] = x[i] * r[i] / r2[i]; y[i] = y[i] * r[i] / r2[i]; z[i] = z[i] * r[i] / r2[i];
					Orbs[i].transform.localPosition = new Vector3(x[i], y[i], z[i]);

					if (ready && !(solved == Display.Length))
					{
						nr[i] = (cr[6 - i] / 2f + ((Display[solved] / j) % 2) / 2f + 63f * nr[i]) / 64f;
						ng[i] = (cg[6 - i] / 2f + ((Display[solved] / j) % 2) / 2f + 63f * ng[i]) / 64f;
						nb[i] = (cb[6 - i] / 2f + ((Display[solved] / j) % 2) / 2f + 63f * nb[i]) / 64f;
						if (t % 30 == 0 && i == 6 - cbcount && colorblind)
						{
							Sphere.GetComponentInChildren<TextMesh>().text = col[7 - i] + bri[((Display[solved] / j) % 2)].ToString();
							cbcount++;
						}
					}
					else
					{
						if (struck)
						{
							nr[i] = (cr[6 - i] / 2f + ((targetNum / j) % 2) / 2f + 63f * nr[i]) / 64f;
							ng[i] = (cg[6 - i] / 2f + ((targetNum / j) % 2) / 2f + 63f * ng[i]) / 64f;
							nb[i] = (cb[6 - i] / 2f + ((targetNum / j) % 2) / 2f + 63f * nb[i]) / 64f;
							if (t % 50 == 0 && i == 6 - cbcount && colorblind)
							{
								Sphere.GetComponentInChildren<TextMesh>().text = col[7 - i] + bri[((targetNum / j) % 2)].ToString();
								cbcount++;
							}
						}
						else
						{
							if (i == 0 && vmod2 > 0.1f && ready)
								vmod2 = vmod2 * 0.99f;
							if (i > direction - 1)
							{
								nr[i] = (1f + 63f * nr[i]) / 64f; ng[i] = (1f + 63f * ng[i]) / 64f; nb[i] = (1f + 63f * nb[i]) / 64f;
							}
							else
							{
								nr[i] = (cr[direction - 1] + 63f * nr[i]) / 64f; ng[i] = (cg[direction - 1] + 63f * ng[i]) / 64f; nb[i] = (cb[direction - 1] + 63f * nb[i]) / 64f;
							}
							if (colorblind)
							{
								Sphere.GetComponentInChildren<TextMesh>().text = direction.ToString() + col[direction];
							}
						}
					}
					Orbs[i].GetComponent<MeshRenderer>().material.color = new Color(nr[i], ng[i], nb[i]);
					Glow[i].GetComponent<Light>().color = new Color(nr[i], ng[i], nb[i], 0.75f);
					j = j * 2;
					if (!colorblind)
						Sphere.GetComponentInChildren<TextMesh>().text = "";
					if (cruel)
						Sphere.GetComponentInChildren<TextMesh>().color = new Color(0.03125f, 0f, 0.0625f);
					else
						Sphere.GetComponentInChildren<TextMesh>().color = new Color(1f, 1f, 1f);
				}
			}
		}
		CruelBreak:;
	}

	private IEnumerator SolvingKugel ()
	{
		Sphere.GetComponentInChildren<TextMesh>().text = "";
		solve = true;
		basecol = new float[] { 1f, 1f, 1f };
		KMAudio.KMAudioRef noise = null;
		for (int i = -10; scale >= 0.01f; i++)
		{
			if (i == -5)
			{
				sound.StopSound();
				sound = null;
				noise = Audio.PlaySoundAtTransformWithRef("Decay", Module.transform);
			}
			Sphere.GetComponentInChildren<TextMesh>().text = "";
			Sphere.GetComponent<MeshRenderer>().material.color = new Color(basecol[0], basecol[1], basecol[2]);
			Shiny.GetComponent<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
			scale -= i / 5000f;
			yield return new WaitForSeconds(0.02f);
			basecol[0] = (statuslight[0] + basecol[0] * 31f) / 32f; basecol[1] = (statuslight[1] + basecol[1] * 31f) / 32f; basecol[2] = (statuslight[2] + basecol[2] * 31f) / 32f;
			for (int j = 0; j < 7; j++)
			{
				x[j] *= 0.92f; y[j] *= 0.92f; z[j] *= 0.92f;
				Orbs[j].transform.localPosition = new Vector3(x[j], y[j], z[j]);
			}
			Sphere.transform.localScale = new Vector3(scale, scale, scale);
			Shiny.GetComponent<Light>().range = scale / 2f + 0.05f;
		}
		Sphere.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
		Shiny.GetComponent<Light>().range = 0.0375f;
		for (int j = 0; j < 7; j++)
		{
			Orbs[j].transform.localPosition = new Vector3(0f, 0f, 0f);
			Orbs[j].transform.localScale = new Vector3( 0f, 0f, 0f );
			Glow[j].GetComponent<Light>().range = 0f;
		}
		Sphere.GetComponent<MeshRenderer>().material.color = new Color(statuslight[0], statuslight[1], statuslight[2]);
		Shiny.GetComponent<Light>().color = new Color(statuslight[0], statuslight[1], statuslight[2], 0.5f);
		noise.StopSound();
		noise = null;
		if (cruel)
		{
			KMSelectable clone;
			clone = Instantiate(Sphere, Module.transform);
			float[] clonecol = { 1f, 1f, 1f };
			for (int i = 0; i < 25; i++)
			{
				basecol[0] = (1f + basecol[0] * 31f) / 32f; basecol[1] = (0.5f + basecol[1] * 31f) / 32f; basecol[2] = (0f + basecol[2] * 31f) / 32f;
				clonecol[0] = (0f + clonecol[0] * 31f) / 32f; clonecol[1] = (0.5f + clonecol[1] * 31f) / 32f; clonecol[2] = (1f + clonecol[2] * 31f) / 32f;
				Sphere.GetComponent<MeshRenderer>().material.color = new Color(basecol[0], basecol[1], basecol[2]);
				clone.GetComponent<MeshRenderer>().material.color = new Color(clonecol[0], clonecol[1], clonecol[2]);
				Sphere.GetComponentInChildren<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
				clone.GetComponentInChildren<Light>().color = new Color(clonecol[0], clonecol[1], clonecol[2], 0.5f);
				Sphere.transform.localPosition = new Vector3(Sphere.transform.localPosition.x - 0.001f, Sphere.transform.localPosition.y, Sphere.transform.localPosition.z);
				clone.transform.localPosition = new Vector3(clone.transform.localPosition.x + 0.001f, clone.transform.localPosition.y, clone.transform.localPosition.z);
				yield return new WaitForSeconds(0.02f);
			}
		}
		Module.HandlePass();
	}

	private IEnumerator StartingKugel()
	{
		scale = 0.05f;
		basecol = new float[] { 0f, 1f, 0f };
		for (int i = -10; scale < 0.1f; i++)
		{
			if(i == -9)
				sound = Audio.PlaySoundAtTransformWithRef("HighPitch", Module.transform);
			Sphere.GetComponent<MeshRenderer>().material.color = new Color(basecol[0], basecol[1], basecol[2]);
			Shiny.GetComponent<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
			scale += i / 25000f;
			yield return new WaitForSeconds(0.02f);
			basecol[0] = (1f + basecol[0] * 63f) / 64f; basecol[1] = (1f + basecol[1] * 63f) / 64f; basecol[2] = (1f + basecol[2] * 63f) / 64f;
			Sphere.transform.localScale = new Vector3(scale, scale, scale);
			Shiny.GetComponent<Light>().range = (i + 140) / 1000f;
			Shiny.GetComponent<Light>().intensity = (i + 240) / 200f;
		}
		Sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		Shiny.GetComponent<Light>().range = 0.1f;
		Shiny.GetComponent<Light>().intensity = 1f;
		scale = 0.1f;
		sound.StopSound();
		sound = null;
	}

	private IEnumerator KugelNextStage()
	{
		KMAudio.KMAudioRef pitch = Audio.PlaySoundAtTransformWithRef("HighPitch", Module.transform);
		yield return new WaitForSeconds(0.5f);
		pitch.StopSound();
		pitch = null;
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, '!{0} h/r/i' to hold, release or toggle interaction with the sphere '!{0} p/t' to wait for a pulse/ tick from the module. Please do not do stupid interactions like '!{0} rphh' or '!{0} iiiii'. Commands can be chained for the sake of solvability.";
#pragma warning restore 414
	private IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant();
		if (command == "colorblind") { colorblind = !colorblind; }
		else
		{
			if(!(solved == Display.Length)) 
			{
				yield return "sendtochaterror {0}, you might want to wait until it's actually ready.";
				yield break;
			}
			string validCommands = "ihrpt";
			bool holding = false;
			for (int i = 0; i < command.Length; i++)
			{
				if ((command[i] == 'h' && holding) || (command[i] == 'r' && !holding))
				{
					yield return "sendtochaterror {0}, you messed up somewhere and made it invalid. Try again.";
					yield break;
				}
				else
				{
					if (command[i] == 'h')
						holding = true;
					if (command[i] == 'r')
						holding = false;
					if (command[i] == 'i')
						holding = !holding;
				}
				if (!validCommands.Contains(command[i]))
				{
					yield return "sendtochaterror {0}, " + command[i] + " is not a valid command.";
					yield break;
				}
			}
			if (holding)
			{
				yield return "sendtochaterror {0}, you shouldn't be holding this thing forever. It's not safe. Mind checking those rules?";
				yield break;
			}
			while (t >= 25) { yield return new WaitForSeconds(0.25f); }
			for (int i = 0; i < command.Length; i++)
			{
				yield return null;
				while (t < 25) { yield return new WaitForSeconds(0.5f); }
				if (command[i] == 'i') { if (!holding) { Sphere.OnInteract(); } else { Sphere.OnInteractEnded(); } holding = !holding; }
				if (command[i] == 'h') { Sphere.OnInteract(); holding = true; }
				if (command[i] == 'r') { Sphere.OnInteractEnded(); holding = false; }
				if (command[i] == 'p' || command[i] == 't') { while (t >= 25) { yield return new WaitForSeconds(0.25f); } }
				yield return new WaitForSeconds(0.5f);
			}
			yield return "strike";
			yield return "solve";
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		while (!(solved == Display.Length)) { yield return new WaitForSeconds(0.5f); }
		while (t >= 25) { yield return new WaitForSeconds(0.25f); }
		for (int i = 0; i < controlSequence.Replace("]∙∙", "]").Length; i++)
		{
			yield return true;
			while (t < 25) { yield return new WaitForSeconds(0.5f); }
			if (controlSequence.Replace("]∙∙", "]")[i] == '[') { Sphere.OnInteract(); }
			if (controlSequence.Replace("]∙∙", "]")[i] == ']') { Sphere.OnInteractEnded(); }
			if (controlSequence.Replace("]∙∙", "]")[i] == '∙') { while (t >= 25) { yield return new WaitForSeconds(0.25f); } }
			yield return new WaitForSeconds(0.5f);
		}
		statuslight = new float[] { 0.5f, 0.25f, 1f };
		userSequence = controlSequence.Replace("]∙∙", "]");
		yield return new WaitForSeconds(3f);
	}
}
