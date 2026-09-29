using System;
using UnityEngine;

namespace KerbalCreator
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class Class1 : MonoBehaviour
    {
        private Rect windowRect = new Rect(100, 100, 350, 450);
        private int windowID = 8521;
        private bool showWindow = false;

        // Basic Info
        private string firstName = "Bob";
        private string lastName = "Kerman";
        private int selectedGenderIndex = 0;
        private string[] genders = { "Male", "Female" };

        private int selectedTraitIndex = 0;
        private string[] traits = { "Pilot", "Engineer", "Scientist" };

        // Advanced Options
        private bool showAdvanced = false;
        private float stupidity = 0.5f;
        private float courage = 0.5f;
        private bool isBadass = false;

        private void Awake()
        {
            GameEvents.onGUIAstronautComplexSpawn.Add(OnAstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Add(OnAstronautComplexDespawn);
        }

        private void OnDestroy()
        {
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnAstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnAstronautComplexDespawn);
        }

        private void OnAstronautComplexSpawn()
        {
            showWindow = true;
        }

        private void OnAstronautComplexDespawn()
        {
            showWindow = false;
        }

        private void OnGUI()
        {
            if (!showWindow) return;

            GUI.skin = HighLogic.Skin;
            windowRect = GUILayout.Window(windowID, windowRect, DrawWindowContent, "Kerbal Creator");
        }

        private void DrawWindowContent(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            GUILayout.Label("First Name:");
            firstName = GUILayout.TextField(firstName);

            GUILayout.Label("Last Name (Default is Kerman):");
            lastName = GUILayout.TextField(lastName);

            GUILayout.Label("Gender:");
            selectedGenderIndex = GUILayout.Toolbar(selectedGenderIndex, genders);

            GUILayout.Label("Profession:");
            selectedTraitIndex = GUILayout.Toolbar(selectedTraitIndex, traits);

            GUILayout.Space(10);

            showAdvanced = GUILayout.Toggle(showAdvanced, "Advanced Options");

            if (showAdvanced)
            {
                GUILayout.Box("", GUILayout.Height(2));

                GUILayout.Label($"Stupidity: {stupidity:F2}");
                stupidity = GUILayout.HorizontalSlider(stupidity, 0f, 1f);

                GUILayout.Label($"Courage: {courage:F2}");
                courage = GUILayout.HorizontalSlider(courage, 0f, 1f);

                isBadass = GUILayout.Toggle(isBadass, "Badass (BadS)");
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Spawn Kerbal", GUILayout.Height(40)))
            {
                SpawnCustomKerbal();
            }

            GUILayout.EndVertical();
        }

        private void SpawnCustomKerbal()
        {
            try
            {
                string fullName = $"{firstName} {lastName}".Trim();

                // 1. Oyundan geçici bir rastgele prototip çekiyoruz (Constructor hatasını aşmak için)
                ProtoCrewMember templateKerbal = CrewGenerator.RandomCrewMemberPrototype(ProtoCrewMember.KerbalType.Crew);

                // 2. Bu prototipi tamamen boş ve bağımsız bir ConfigNode hücresine kaydediyoruz
                ConfigNode node = new ConfigNode("KERBAL");
                templateKerbal.Save(node);

                // 3. Şablonun üzerindeki değerleri tamamen kendi girdiğimiz UI verileriyle eziyoruz
                node.SetValue("name", fullName);
                node.SetValue("gender", genders[selectedGenderIndex]);
                node.SetValue("trait", traits[selectedTraitIndex]);
                node.SetValue("bravery", courage.ToString());
                node.SetValue("stupidity", stupidity.ToString());
                node.SetValue("badS", isBadass.ToString().ToLower());
                node.SetValue("rosterStatus", "Available");

                // 4. KSP'nin her sürümünde çalışan resmi yükleme metodu ile yepyeni nesneyi üretiyoruz
                ProtoCrewMember newKerbal = new ProtoCrewMember(HighLogic.CurrentGame.Mode, node, ProtoCrewMember.KerbalType.Crew);

                // 5. Roster'a ekle
                HighLogic.CurrentGame.CrewRoster.AddCrewMember(newKerbal);

                ScreenMessages.PostScreenMessage($"{fullName} has joined the roster as a new recruit!", 4.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            catch (Exception e)
            {
                Debug.LogError($"[KerbalCreator] Error spawning Kerbal via Safe ConfigNode: {e.Message}");
            }
        }
    }
}
