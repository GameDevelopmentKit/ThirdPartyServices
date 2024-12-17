namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

#pragma warning disable 0618
    /// <summary>
    /// An ad placement represents an exact location to serve an ad unit in your app.
    /// </summary>
    [Serializable]
    public class AdPlacement : AdLocation
    {
        // Stores all custom placements, built-in or user-created.
        private static Dictionary<string, AdPlacement> sCustomPlacements = new();

        [SerializeField] [LabelText("Name")] protected string mName;

        /// <summary>
        /// The name of the placement.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.mName;

        protected AdPlacement()
        {
        }

        private AdPlacement(string name, bool isDefault = false)
            : base(name, false)
        {
            this.mName = name;

            if (!isDefault) sCustomPlacements[name] = this;
        }

        #region Built-in Placements.

        /// <summary>
        /// A placement with an empty name. Whenever you attempt to create a new
        /// placement with a null or empty name, this one will be returned.
        /// </summary>
        public new static readonly AdPlacement Default = new(string.Empty, true);

        /// <summary>
        /// A placement with the name "Startup".
        /// </summary>
        public new static readonly AdPlacement Startup = new("Startup");

        /// <summary>
        /// A placement with the name "Home_Screen".
        /// </summary>
        public new static readonly AdPlacement HomeScreen = new("Home_Screen");

        /// <summary>
        /// A placement with the name "Main_Menu".
        /// </summary>
        public new static readonly AdPlacement MainMenu = new("Main_Menu");

        /// <summary>
        /// A placement with the name "Game_Screen".
        /// </summary>
        public new static readonly AdPlacement GameScreen = new("Game_Screen");

        /// <summary>
        /// A placement with the name "Achievements".
        /// </summary>
        public new static readonly AdPlacement Achievements = new("Achievements");

        /// <summary>
        /// A placement with the name "Level_Start".
        /// </summary>
        public new static readonly AdPlacement LevelStart = new("Level_Start");

        /// <summary>
        /// A placement with the name "Level_Complete".
        /// </summary>
        public new static readonly AdPlacement LevelComplete = new("Level_Complete");

        /// <summary>
        /// A placement with the name "Turn_Complete".
        /// </summary>
        public new static readonly AdPlacement TurnComplete = new("Turn_Complete");

        /// <summary>
        /// A placement with the name "Quests".
        /// </summary>
        public new static readonly AdPlacement Quests = new("Quests");

        /// <summary>
        /// A placement with the name "Pause".
        /// </summary>
        public new static readonly AdPlacement Pause = new("Pause");

        /// <summary>
        /// A placement with the name "IAP_Store".
        /// </summary>
        public new static readonly AdPlacement IAPStore = new("IAP_Store");

        /// <summary>
        /// A placement with the name "Item_Store".
        /// </summary>
        public new static readonly AdPlacement ItemStore = new("Item_Store");

        /// <summary>
        /// A placement with the name "Game_Over".
        /// </summary>
        public new static readonly AdPlacement GameOver = new("Game_Over");

        /// <summary>
        /// A placement with the name "Leaderboard".
        /// </summary>
        public static readonly AdPlacement Leaderboard = new("Leaderboard");

        /// <summary>
        /// A placement with the name "Settings".
        /// </summary>
        public new static readonly AdPlacement Settings = new("Settings");

        /// <summary>
        /// A placement with the name "Quit".
        /// </summary>
        public new static readonly AdPlacement Quit = new("Quit");

        #endregion // Built-in Placements

        /// <summary>
        /// Gets all existing placements including <c>AdPlacement.Default</c>.
        /// </summary>
        /// <returns>The all placements.</returns>
        public static AdPlacement[] GetAllPlacements()
        {
            var list = new List<AdPlacement>();
            list.Add(Default);
            list.AddRange(sCustomPlacements.Values);
            return list.ToArray();
        }

        /// <summary>
        /// Gets all existing placements excluding <c>AdPlacement.Default</c>.
        /// </summary>
        /// <returns>The custom placements.</returns>
        public static AdPlacement[] GetCustomPlacements()
        {
            var list = new List<AdPlacement>(sCustomPlacements.Values);
            return list.ToArray();
        }

        /// <summary>
        /// Returns a new placement with the given name, or an
        /// existing placement with that name if one exists.
        /// If a null or empty name is given, the <c>AdPlacement.Default</c> placement will be returned.
        /// </summary>
        /// <returns>The placement.</returns>
        /// <param name="name">Name.</param>
        public static AdPlacement PlacementWithName(string name)
        {
            if (string.IsNullOrEmpty(name)) return Default;

            if (sCustomPlacements.ContainsKey(name)) return sCustomPlacements[name] as AdPlacement;

            return new(name);
        }

        public static string GetPrintableName(AdPlacement placement)
        {
            return placement == null ? "null" : placement == Default ? "[Default]" : placement.ToString();
        }

        public override string ToString()
        {
            return this.mName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AdPlacement;

            if (other == null) return false;

            if (string.IsNullOrEmpty(this.Name)) return string.IsNullOrEmpty(other.Name);

            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public static bool operator ==(AdPlacement placementA, AdPlacement placementB)
        {
            if (ReferenceEquals(placementA, null)) return ReferenceEquals(placementB, null);

            return placementA.Equals(placementB);
        }

        public static bool operator !=(AdPlacement placementA, AdPlacement placementB)
        {
            return !(placementA == placementB);
        }
    }
#pragma warning restore 0618
}