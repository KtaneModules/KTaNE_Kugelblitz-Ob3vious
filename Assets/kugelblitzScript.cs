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
	public List<MeshRenderer> OrbsMeshRenderer;
	public GameObject[] Glow;
	public List<Light> GlowLight;
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
	static int struck = 0;

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
	private static int[][] DisplayBase = new int[0][];
	private int[] Display;
	public static int targetNum;
	public static int direction;
	public static bool totalsolved;
	private int[,] grid = {	{ 5, 1, 3, 6, 4, 0, 2},
							{ 1, 2, 6, 4, 0, 5, 3},
							{ 4, 0, 5, 1, 3, 2, 6},
							{ 3, 5, 2, 0, 1, 6, 4},
							{ 0, 3, 4, 2, 6, 1, 5},
							{ 6, 4, 0, 5, 2, 3, 1},
							{ 2, 6, 1, 3, 5, 4, 0} };
	private int[] pivot = { 0, 0, 0 };
	private int[] storedb = new int[0];
	private int[] xtransf = { 0, 1, 1, 1, 0, -1, -1, -1 };
	private int[] ytransf = { -1, -1, 0, 1, 1, 1, 0, -1 };
	private int rotation = 1;
	private string userSequence = "";
	public static string controlSequence = "";
	private string binary = "";
	private float t = 0;
	private int solved = 0;
	private int memcount = 0;
	private float[] basecol = { 0.03125f, 0f, 0.0625f };
	private float scale = 0.1f;

	private bool solve = false;
	private bool hold = false;

	private bool colorblind;
	public static int modID = 1;
	public int currentModID;

	public static int kugelcount;
	public int kugelID;

	static List<int> extras = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
	private int[] red = new int[7];
	private int[] orange = new int[7];
	private int[] yellow = new int[7];
	private int[] green = { 0, 1, 2, 3, 4, 5, 6 };
	private int[] blue = { 2, 2, 2, 2, 0, 2, 2 };
	private int[] indigo = new int[7];
	private int[][] violet = new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0, 0 } };

	private MeshRenderer sphereMeshRenderer;
	private TextMesh sphereTextMesh;
	private Light shinyMeshRenderer;

	void Awake () {
		kugelcount = 0;
		struck = 0;
		totalsolved = false;
		controlSequence = "[";
		sphereMeshRenderer = Sphere.GetComponent<MeshRenderer>();
		sphereTextMesh = Sphere.GetComponentInChildren<TextMesh>();
		shinyMeshRenderer = Shiny.GetComponent<Light>();
		foreach (GameObject orb in Orbs)
		{
			OrbsMeshRenderer.Add(orb.GetComponent<MeshRenderer>());
		}

		foreach (GameObject light in Glow)
		{
			GlowLight.Add(light.GetComponent<Light>());
		}
		
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
				"Organization",
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
				Module.HandleStrike(); sphereMeshRenderer.material.color = new Color(0.5f, 0f, 0f); basecol = new float[] { 0.5f, 0f, 0f }; hold = false;
				struck = (struck + 1) % (solved + 1);
			}
			else
			{
				if (!solve)
				{
					sphereMeshRenderer.material.color = new Color(1f, 1f, 1f);
					shinyMeshRenderer.color = new Color(1f, 1f, 1f, 0.5f);
					basecol = new float[] { 1f, 1f, 1f };
				}
				hold = true; 
				userSequence += '[';
				if (userSequence == "[")
				{
					sound = Audio.PlaySoundAtTransformWithRef("VoidSucc", Module.transform);
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
			}
		};
		BombInfo.OnBombExploded += delegate ()
		{
			if(sound != null)
			{
				sound.StopSound();
				sound = null;
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
			vmod[i] = 0.001f / Mathf.Pow(r[i], 2f);
		}
		sphereTextMesh.text = "";
	}

	private void Start()
	{
		kugelcount++;
		kugelID = kugelcount;
		StartCoroutine(StartingKugel());
	}

	private void ActivateModule()
	{
		int count = BombInfo.GetSolvableModuleNames().Count(a => !ignoredModules.Contains(a)) + ADDED_STAGES;
		string[] cardinals = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest"};
		if (count == 0) { Module.HandlePass(); forcedSolve = true; } //Prevent deadlock
		else if (kugelID == 1)
		{
			Debug.LogFormat("[Kugelblitz #{0}] Found {1} solvable modules.", currentModID, count);
			extras = new int[] { 1, 2, 3, 4, 5, 6, 7 }.Shuffle().Take(kugelcount - 1).ToList();
			DisplayBase = new int[kugelcount][];
			for (int i = 0; i < DisplayBase.Length; i++)
			{
				DisplayBase[i] = new int[count];
			}
			targetNum = Rnd.Range(0, 1) * 64 + Rnd.Range(0, 7) * 8 + Rnd.Range(0, 7);
			int previous = 0;
			for (int i = 0; i < count - 1; i++)
			{
				DisplayBase[0][i] = Rnd.Range(0, 128);
				previous = DisplayBase[0][i] ^ previous;
			}
			DisplayBase[0][count - 1] = targetNum ^ previous;
			direction = Rnd.Range(0, 8);
			pivot[0] = (targetNum / 8) % 8;
			pivot[1] = targetNum % 8;
			pivot[2] = direction % 8;
			if (targetNum / 64 == 1)
				rotation *= -1;
			Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers are {1}. This will result in {2}.", currentModID, DisplayBase[0].Select(x => Binarify(x)).Join(", "), Binarify(targetNum));
			for (int i = 0; i < extras.ToArray().Length; i++)
			{
				HandleQuirks(extras[i], count, i + 1);
			}
			Debug.LogFormat("[Kugelblitz #{0}] Starting at ({1}, {2}) in direction {3}.", currentModID, pivot[0], pivot[1], cardinals[direction]);
			blue = new int[] { 0, 1, 2, 3, 5, 6 }.Select(x => blue[x]).Concat(new int[] { 0 }).ToArray();
			for (int i = 0; i < 7; i++)
			{
				storedb = storedb.Concat(new int[] { 0 }).ToArray();
				for (int j = 0; j <= green[i]; j++)
				{
					//Debug.LogFormat("[Kugelblitz #{0}] ({1}, {2}), {3}.", currentModID, pivot[0], pivot[1], grid[pivot[1], pivot[0]]);
					pivot[2] = (pivot[2] + 16) % 8;
					storedb[i] += grid[pivot[1], pivot[0]] + violet[0][pivot[0]] + violet[1][pivot[1]];
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
				rotation *= 1 - (2 * indigo[i]);
				pivot[2] = pivot[2] + rotation * (blue[i] + 1);
				storedb[i] %= 7;
			}
			Debug.LogFormat("[Kugelblitz #{0}] Calculated values are {1}.", currentModID, storedb.Join(""));
            if (extras.Contains(1))
            {
                for (int i = 0; i < 7; i++)
                {
					storedb[i] = (storedb[i] + red[i]) % 7;
                }
				Debug.LogFormat("[Kugelblitz #{0}] Modified values (red) are {1}.", currentModID, storedb.Join(""));
			}
			for (int i = 0; i < storedb.Length; i++)
			{
				string[] binarystuff = { "000", "001", "010", "011", "100", "101", "110" };
				binary += binarystuff[storedb[i]];
			}
			Debug.LogFormat("[Kugelblitz #{0}] The calculated binary is 1{1}.", currentModID, binary);
			if (extras.Contains(3))
			{
				string tempbinary = "";
				for (int i = 0; i < 7; i++)
				{
					tempbinary += yellow[i] + binary.Substring(i * 3, 3);
				}
				binary = tempbinary;
				Debug.LogFormat("[Kugelblitz #{0}] Modified binary (yellow) is 1{1}.", currentModID, binary);
			}
			if (extras.Contains(2))
			{
				string tempbinary = "";
				for (int i = 0; i < 7; i++)
				{
					if (extras.Contains(3))
					{
						for (int j = 0; j < 4; j++)
						{
							if (binary[i * 4 + j].ToString() != orange[i].ToString())
								tempbinary += "1";
							else
								tempbinary += "0";
						}
					}
					else
					{
						for (int j = 0; j < 3; j++)
						{
							if (binary[i * 3 + j].ToString() != orange[i].ToString())
								tempbinary += "1";
							else
								tempbinary += "0";
						}
					}
				}
				binary = tempbinary;
				Debug.LogFormat("[Kugelblitz #{0}] Modified binary (orange) is 1{1}.", currentModID, binary);
			}
			int k = 1;
			bool h = true;
			for (int i = 0; i < binary.Length; i++)
			{
				if (binary[i] == '1')
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
						{
							controlSequence += ']';
						}
						else
						{
							controlSequence += '[';
						}
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
						{
							controlSequence += '[';
						}
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
			}
			if (h)
			{
				controlSequence += "]∙∙";
			}
			else
			{
				if (controlSequence[controlSequence.Length - 1] != '∙' || controlSequence[controlSequence.Length - 2] != '∙')
				{
					while (controlSequence[controlSequence.Length - 1] != '∙' || controlSequence[controlSequence.Length - 2] != '∙')
						controlSequence += '∙';
				}
			}
			Debug.LogFormat("[Kugelblitz #{0}] The calculated sequence is {1}, which translates to {2}.", currentModID, controlSequence.Replace("]∙∙", "]").Replace("[", "i").Replace("]", "i").Replace("∙", "p"), controlSequence.Replace("]∙∙", "]"));
		}
		else
		{
			Debug.LogFormat("[Kugelblitz #{0}] Logging can be found at 'Kugelblitz #{1}'.", currentModID, (currentModID - kugelID + 1));
		}
		Display = new int[count];
		ready = true;
	}
		
	IEnumerator Run () {
		if (!forcedSolve)
		{
			while (!solve)
			{
				for (t = 0; t < 2.5f; t += Time.deltaTime)
				{
					if (totalsolved)
					{
						statuslight = new float[] { 0.75f, 0.75f, 0.75f };
						StartCoroutine(SolvingKugel());
						yield break;
					}
					else
					{
						solved = BombInfo.GetSolvedModuleNames().Where(a => !ignoredModules.Contains(a)).Count();
						if (memcount != solved) { memcount = solved; struck = 0; StartCoroutine(KugelNextStage()); }
						int j = 1;

						string col = "-ROYGBIV";
						string bri = "-+";
						for (int i = 0; i < 7; i++)
						{
							r2[i] = Mathf.Pow(Mathf.Pow(x[i] + vx[i] * Time.deltaTime, 2f) + Mathf.Pow(y[i] + vy[i] * Time.deltaTime, 2f) + Mathf.Pow(z[i] + vz[i] * Time.deltaTime, 2f), 0.5f);
							vx[i] = (x[i] + vx[i] * Time.deltaTime) * r[i] / r2[i]; vy[i] = (y[i] + vy[i] * Time.deltaTime) * r[i] / r2[i]; vz[i] = (z[i] + vz[i] * Time.deltaTime) * r[i] / r2[i];
							vx[i] = vx[i] - x[i]; vy[i] = vy[i] - y[i]; vz[i] = vz[i] - z[i];
							r2[i] = Mathf.Pow(Mathf.Pow(vx[i], 2f) + Mathf.Pow(vy[i], 2f) + Mathf.Pow(vz[i], 2f), 0.5f);
							vx[i] = vx[i] * vmod[i] * vmod2 / r2[i]; vy[i] = vy[i] * vmod[i] * vmod2 / r2[i]; vz[i] = vz[i] * vmod[i] * vmod2 / r2[i];
							x[i] = x[i] + vx[i] * Time.deltaTime; y[i] = y[i] + vy[i] * Time.deltaTime; z[i] = z[i] + vz[i] * Time.deltaTime;
							r2[i] = Mathf.Pow(Mathf.Pow(x[i], 2f) + Mathf.Pow(y[i], 2f) + Mathf.Pow(z[i], 2f), 0.5f);
							x[i] = x[i] * r[i] / r2[i]; y[i] = y[i] * r[i] / r2[i]; z[i] = z[i] * r[i] / r2[i];
							Orbs[i].transform.localPosition = new Vector3(x[i], y[i], z[i]);

							if (ready && !(solved == Display.Length))
							{
								for (int k = 0; k < Display.Length; k++)
								{
									Display[k] = DisplayBase[kugelID - 1][k];
								}
								if (((Display[solved - struck] / j) % 2 == 1))
								{
									if (kugelID > 1 && kugelID < 9 && i == 7 - extras[kugelID - 2])
									{
										nr[i] = (63f * nr[i]) / 64f;
										ng[i] = (63f * ng[i]) / 64f;
										nb[i] = (63f * nb[i]) / 64f;
									}
									else
									{
										nr[i] = (cr[6 - i] + 63f * nr[i]) / 64f;
										ng[i] = (cg[6 - i] + 63f * ng[i]) / 64f;
										nb[i] = (cb[6 - i] + 63f * nb[i]) / 64f;
									}
								}
								else
								{
									if ((solved - struck) % 2 == 1 && kugelID < 9)
									{
										if (kugelID > 1 && kugelID < 9)
										{
											nr[i] = (cr[extras[kugelID - 2] - 1] + 63f * nr[i]) / 64f;
											ng[i] = (cg[extras[kugelID - 2] - 1] + 63f * ng[i]) / 64f;
											nb[i] = (cb[extras[kugelID - 2] - 1] + 63f * nb[i]) / 64f;
										}
										else
										{
											nr[i] = (63f * nr[i]) / 64f;
											ng[i] = (63f * ng[i]) / 64f;
											nb[i] = (63f * nb[i]) / 64f;
										}
									}
									else
									{
										nr[i] = (1f + 63f * nr[i]) / 64f;
										ng[i] = (1f + 63f * ng[i]) / 64f;
										nb[i] = (1f + 63f * nb[i]) / 64f;
									}
								}
								if (i == 6 - (int)(t * 2.8f) && colorblind)
                                {
									if (kugelID == 1)
									{
										sphereTextMesh.color = new Color(1f, 1f, 1f);
										sphereTextMesh.text = col[7 - i] + bri[((Display[solved - struck] / j) % 2)].ToString();
									}
									else if (kugelID < 9)
									{
										sphereTextMesh.color = new Color(0f, 0f, 0f);
										if (i == 7 - extras[kugelID - 2])
											sphereTextMesh.text = col[extras[kugelID - 2]] + "\nK" + bri[((Display[solved - struck] / j) % 2)].ToString();
										else
											sphereTextMesh.text = col[extras[kugelID - 2]] + "\n" + col[7 - i] + bri[((Display[solved - struck] / j) % 2)].ToString();
									}
								}
							}
							else
							{
								if (struck > 0)
								{
									if (((Display[solved - struck] / j) % 2 == 1))
									{
										if (kugelID > 1 && kugelID < 9 && i == 7 - extras[kugelID - 2])
										{
											nr[i] = (63f * nr[i]) / 64f;
											ng[i] = (63f * ng[i]) / 64f;
											nb[i] = (63f * nb[i]) / 64f;
										}
										else
										{
											nr[i] = (cr[6 - i] + 63f * nr[i]) / 64f;
											ng[i] = (cg[6 - i] + 63f * ng[i]) / 64f;
											nb[i] = (cb[6 - i] + 63f * nb[i]) / 64f;
										}
									}
									else
									{
										if ((solved - struck) % 2 == 1 && kugelID < 9)
										{
											if (kugelID > 1 && kugelID < 9)
											{
												nr[i] = (cr[extras[kugelID - 2] - 1] + 63f * nr[i]) / 64f;
												ng[i] = (cg[extras[kugelID - 2] - 1] + 63f * ng[i]) / 64f;
												nb[i] = (cb[extras[kugelID - 2] - 1] + 63f * nb[i]) / 64f;
											}
											else
											{
												nr[i] = (63f * nr[i]) / 64f;
												ng[i] = (63f * ng[i]) / 64f;
												nb[i] = (63f * nb[i]) / 64f;
											}
										}
										else
										{
											nr[i] = (1f + 63f * nr[i]) / 64f;
											ng[i] = (1f + 63f * ng[i]) / 64f;
											nb[i] = (1f + 63f * nb[i]) / 64f;
										}
									}
									if (i == 6 - (int)(t * 2.8f) && colorblind)
									{
										if (kugelID > 1 && kugelID < 9 && i == 7 - extras[kugelID - 2])
											sphereTextMesh.text = col[extras[kugelID - 2]] + "\nK" + bri[((Display[solved - struck] / j) % 2)].ToString();
										else if (kugelID > 1 && kugelID < 9)
											sphereTextMesh.text = col[extras[kugelID - 2]] + "\n" + col[7 - i] + bri[((Display[solved - struck] / j) % 2)].ToString();
										else
											sphereTextMesh.text = col[7 - i] + bri[((Display[solved - struck] / j) % 2)].ToString();
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
										sphereTextMesh.text = direction.ToString() + col[direction];
									}
								}
							}
							OrbsMeshRenderer[i].material.color = new Color(nr[i], ng[i], nb[i], 0.5f);
							GlowLight[i].color = new Color(nr[i], ng[i], nb[i], 0.0625f);
							j = j * 2;
							if (!colorblind)
								sphereTextMesh.text = "";
						}
					}
					if (!hold)
					{
						if (kugelID == 1)
						{
							basecol[0] = (0.03125f + basecol[0] * 63f) / 64f; basecol[1] = (basecol[1] * 63f) / 64f; basecol[2] = (0.0625f + basecol[2] * 63f) / 64f;
						}
						else if (kugelID < 9)
						{
							basecol[0] = (cr[extras[kugelID - 2] - 1] + basecol[0] * 63f) / 64f; basecol[1] = (cg[extras[kugelID - 2] - 1] + basecol[1] * 63f) / 64f; basecol[2] = (cb[extras[kugelID - 2] - 1] + basecol[2] * 63f) / 64f;
						}
                        else
                        {
							basecol[0] = (1f + basecol[0] * 63f) / 64f; basecol[1] = (1f + basecol[1] * 63f) / 64f; basecol[2] = (1f + basecol[2] * 63f) / 64f;
						}
						sphereMeshRenderer.material.color = new Color(basecol[0], basecol[1], basecol[2]);
						shinyMeshRenderer.color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
					}
					yield return null;
				}
				if (ready && (solved == Display.Length))
				{
					for (int i = 0; i < 7; i++)
					{
						nr[i] = 0f; ng[i] = 0f; nb[i] = 0f;
					}
                    if (kugelID == 1)
						Audio.PlaySoundAtTransform("Pulse", Module.transform);
				}
				if (userSequence != "") { userSequence += '∙'; }
				if (userSequence.Length > 2 && !hold)
				{
					if (userSequence[userSequence.Length - 2] == '∙')
					{
						if (userSequence != controlSequence)
						{
							sphereMeshRenderer.material.color = new Color(0.5f, 0f, 0f);
							basecol = new float[] { 0.5f, 0f, 0f };
							Module.HandleStrike();
							Debug.LogFormat("[Kugelblitz #{0}] You submitted {1}, but I expected {2}.", currentModID, userSequence.Replace("]∙∙", "]"), controlSequence.Replace("]∙∙", "]"));
							struck = (struck + 1) % (solved + 1);
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
			}
		}
	}

	private IEnumerator SolvingKugel ()
	{
		sphereTextMesh.text = "";
		solve = true;
		totalsolved = true;
		basecol = new float[] { 1f, 1f, 1f };
		KMAudio.KMAudioRef noise = null;
		for (int i = -10; scale >= 0.01f; i++)
		{
			if (i == -5)
			{
				if (sound != null)
				{
					sound.StopSound();
					sound = null;
				}
				noise = Audio.PlaySoundAtTransformWithRef("Decay", Module.transform);
			}
			sphereTextMesh.text = "";
			sphereMeshRenderer.material.color = new Color(basecol[0], basecol[1], basecol[2]);
			shinyMeshRenderer.color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
			scale -= i / 5000f;
			yield return new WaitForSeconds(0.02f);
			basecol[0] = (statuslight[0] + basecol[0] * 31f) / 32f; basecol[1] = (statuslight[1] + basecol[1] * 31f) / 32f; basecol[2] = (statuslight[2] + basecol[2] * 31f) / 32f;
			for (int j = 0; j < 7; j++)
			{
				x[j] *= 0.92f; y[j] *= 0.92f; z[j] *= 0.92f;
				Orbs[j].transform.localPosition = new Vector3(x[j], y[j], z[j]);
			}
			Sphere.transform.localScale = new Vector3(scale, scale, scale);
			shinyMeshRenderer.range = scale / 2f + 0.05f;
		}
		Sphere.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
		shinyMeshRenderer.range = 0.0375f;
		for (int j = 0; j < 7; j++)
		{
			Orbs[j].transform.localPosition = new Vector3(0f, 0f, 0f);
			Orbs[j].transform.localScale = new Vector3( 0f, 0f, 0f );
			GlowLight[j].range = 0f;
		}
		sphereMeshRenderer.material.color = new Color(statuslight[0], statuslight[1], statuslight[2]);
		shinyMeshRenderer.color = new Color(statuslight[0], statuslight[1], statuslight[2], 0.5f);
		noise.StopSound();
		noise = null;
		/*if (something idk)
		{
			KMSelectable clone;
			clone = Instantiate(Sphere, Module.transform);
			float[] clonecol = { 1f, 1f, 1f };
			for (int i = 0; i < 25; i++)
			{
				basecol[0] = (1f + basecol[0] * 31f) / 32f; basecol[1] = (0.5f + basecol[1] * 31f) / 32f; basecol[2] = (0f + basecol[2] * 31f) / 32f;
				clonecol[0] = (0f + clonecol[0] * 31f) / 32f; clonecol[1] = (0.5f + clonecol[1] * 31f) / 32f; clonecol[2] = (1f + clonecol[2] * 31f) / 32f;
				sphereMeshRenderer.material.color = new Color(basecol[0], basecol[1], basecol[2]);
				//Justified GetComponent in this case
				clone.GetComponent<MeshRenderer>().material.color = new Color(clonecol[0], clonecol[1], clonecol[2]);
				Sphere.GetComponentInChildren<Light>().color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
				clone.GetComponentInChildren<Light>().color = new Color(clonecol[0], clonecol[1], clonecol[2], 0.5f);
				Sphere.transform.localPosition = new Vector3(Sphere.transform.localPosition.x - 0.001f, Sphere.transform.localPosition.y, Sphere.transform.localPosition.z);
				clone.transform.localPosition = new Vector3(clone.transform.localPosition.x + 0.001f, clone.transform.localPosition.y, clone.transform.localPosition.z);
				yield return new WaitForSeconds(0.02f);
			}
		}*/
		Module.HandlePass();
	}

	private IEnumerator StartingKugel()
	{
		scale = 0.05f;
		basecol = new float[] { 1f, 1f, 1f };
		for (int i = -10; scale < 0.1f; i++)
		{
			if(i == -9 && kugelID == 1)
				sound = Audio.PlaySoundAtTransformWithRef("HighPitch", Module.transform);
			sphereMeshRenderer.material.color = new Color(basecol[0], basecol[1], basecol[2]);
			shinyMeshRenderer.color = new Color(basecol[0], basecol[1], basecol[2], 0.5f);
			scale += i / 25000f;
			yield return new WaitForSeconds(0.02f);
			basecol[0] = (1f + basecol[0] * 63f) / 64f; basecol[1] = (1f + basecol[1] * 63f) / 64f; basecol[2] = (1f + basecol[2] * 63f) / 64f;
			Sphere.transform.localScale = new Vector3(scale, scale, scale);
			shinyMeshRenderer.range = (i + 140) / 1000f;
			shinyMeshRenderer.intensity = (i + 240) / 200f;
		}
		Sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		//shinyMeshRenderer.range = 0.1f;
		//shinyMeshRenderer.intensity = 1f;
		scale = 0.1f;
        if (kugelID == 1)
        {
			sound.StopSound();
			sound = null;
		}
		StartCoroutine(Run());
	}

	private IEnumerator KugelNextStage()
	{
		if (kugelID == 1)
		{
			KMAudio.KMAudioRef pitch = Audio.PlaySoundAtTransformWithRef("HighPitch", Module.transform);
			yield return new WaitForSeconds(1f);
			pitch.StopSound();
			pitch = null;
		}
	}

	private string Binarify(int number)
	{
		return (number / 64).ToString() + ((number / 32) % 2).ToString() + ((number / 16) % 2).ToString() + ((number / 8) % 2).ToString() + ((number / 4) % 2).ToString() + ((number / 2) % 2).ToString() + (number % 2).ToString();
	}

	private void HandleQuirks(int type, int stagecount, int idnumber)
	{
		switch (type)
		{
			case 1:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						red[j] += values[j];
						red[j] %= 7;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (red) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), red.Join(""));
				break;
			case 2:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						orange[j] += values[j];
						orange[j] %= 2;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (orange) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), orange.Join(""));
				break;
			case 3:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						yellow[j] += values[j];
						yellow[j] %= 2;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (yellow) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), yellow.Join(""));
				break;
			case 4:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						green[j] += values[j];
						green[j] %= 7;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (green) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), green.Select(x => x + 1).Join(""));
				break;
			case 5:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						blue[j] += values[j];
						blue[j] %= 3;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
                }
				{
					int[] skipover = { 0, 1, 2, 3, 5, 6 };
					for (int i = 0; i < 6; i++)
					{
						blue[skipover[i]] = (blue[4] + blue[skipover[i]]) % 3;
					}
					Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (blue) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), skipover.Select(x => blue[x] + 1).Join(""));
				}
				break;
			case 6:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						indigo[j] += values[j];
						indigo[j] %= 2;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				{
					int[] skipover = { 0, 1, 2, 3, 4, 6 };
					for (int i = 0; i < 6; i++)
					{
						indigo[skipover[i]] = indigo[5] ^ indigo[skipover[i]];
					}
					indigo = skipover.Select(x => indigo[x]).Concat(new int[] { 0 }).ToArray();
					Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (indigo) are {1}. This will result in {2}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), indigo.Take(6).Join(""));
				}
				break;
			case 7:
				for (int i = 0; i < stagecount; i++)
				{
					int[] values = new int[7];
					for (int j = 0; j < 7; j++)
					{
						values[j] = Rnd.Range(0, 2);
						violet[i % 2][j] += values[j];
						violet[i % 2][j] %= 7;
					}
					int k = 0;
					for (int j = 0; j < 7; j++)
					{
						k = k * 2 + values[j];
					}
					DisplayBase[idnumber][i] = k;
				}
				Debug.LogFormat("[Kugelblitz #{0}] Generated binary numbers for extra Kugelblitz (violet) are {1}. This will result in horzontally placed modifier {2} and vertically placed modifier {3}.", currentModID, DisplayBase[idnumber].Select(x => Binarify(x)).Join(", "), violet[0].Join(""), violet[1].Join(""));
				break;
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, 'h'/'r'/'i' to hold, release or toggle interaction with the sphere (respectively). 'p'/'t' to wait for a pulse/tick from the module. Please do not do stupid interactions like '!{0} rphh' or '!{0} iiiii'. Commands can be chained for the sake of solvability.";
#pragma warning restore 414
	private IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant();
		if (command == "colorblind") { colorblind = !colorblind; }
		else
		{
			/*if(!(solved == Display.Length)) 
			{
				yield return "sendtochaterror {0}, you might want to wait until it's actually ready.";
				yield break;
			}*/
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
			while (t >= 0.5f) { yield return new WaitForSeconds(0.25f); }
			for (int i = 0; i < command.Length; i++)
			{
				yield return null;
				while (t < 0.5f) { yield return null; }
				if (command[i] == 'i') { if (!holding) { Sphere.OnInteract(); } else { Sphere.OnInteractEnded(); } holding = !holding; }
				if (command[i] == 'h') { Sphere.OnInteract(); holding = true; }
				if (command[i] == 'r') { Sphere.OnInteractEnded(); holding = false; }
				if (command[i] == 'p' || command[i] == 't') { while (t >= 0.5f) { yield return null; } }
				yield return null;
			}
			yield return "strike";
			yield return "solve";
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		if (kugelID == 1)
		{
			statuslight = new float[] { 0.5f, 0.25f, 1f };
			while (!(solved == Display.Length)) { yield return true; }
			while (t >= 0.5f) { yield return true; }
			for (int i = 0; i < controlSequence.Replace("]∙∙", "]").Length; i++)
			{
				while (t < 0.5f) { yield return null; }
				if (controlSequence.Replace("]∙∙", "]")[i] == '[') { Sphere.OnInteract(); }
				if (controlSequence.Replace("]∙∙", "]")[i] == ']') { Sphere.OnInteractEnded(); }
				if (controlSequence.Replace("]∙∙", "]")[i] == '∙') { while (t >= 0.5f) { yield return null; } }
				yield return null;
			}
			userSequence = controlSequence.Replace("]∙∙", "]");
		}
	}
}
