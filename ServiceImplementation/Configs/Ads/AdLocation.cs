namespace ServiceImplementation.Configs.Ads
{
    using System;
    using System.Collections;

    [Obsolete("This class was deprecated. Please use EasyMobile.AdPlacement instead.")]
    public class AdLocation
    {
        private readonly string    name;
        private static   Hashtable map = new();

        protected AdLocation()
        {
        }

        protected AdLocation(string name, bool addToMap = true)
        {
            this.name = name;
            if (addToMap) map.Add(name, this);
        }

        /// <summary>
        /// Returns a String that represents the current AdLocation.
        /// </summary>
        /// <returns>A String that represents the current AdLocation</returns>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// Default location.
        /// </summary>
        public static readonly AdLocation Default = new("Default");

        /// <summary>
        /// Initial startup of your app.
        /// </summary>
        public static readonly AdLocation Startup = new("Startup");

        /// <summary>
        /// Home screen the player first sees.
        /// </summary>
        public static readonly AdLocation HomeScreen = new("Home Screen");

        /// <summary>
        /// Menu that provides game options.
        /// </summary>
        public static readonly AdLocation MainMenu = new("Main Menu");

        /// <summary>
        /// Menu that provides game options.
        /// </summary>
        public static readonly AdLocation GameScreen = new("Game Screen");

        /// <summary>
        /// Screen with list achievements in the game.
        /// </summary>
        public static readonly AdLocation Achievements = new("Achievements");

        /// <summary>
        /// Quest, missions or goals screen describing things for a player to do.
        /// </summary>
        public static readonly AdLocation Quests = new("Quests");

        /// <summary>
        /// Pause screen.
        /// </summary>
        public static readonly AdLocation Pause = new("Pause");

        /// <summary>
        /// Start of the level.
        /// </summary>
        public static readonly AdLocation LevelStart = new("Level Start");

        /// <summary>
        /// Completion of the level.
        /// </summary>
        public static readonly AdLocation LevelComplete = new("Level Complete");

        /// <summary>
        /// Finishing a turn in a game.
        /// </summary>
        public static readonly AdLocation TurnComplete = new("Turn Complete");

        /// <summary>
        /// The store where the player pays real money for currency or items.
        /// </summary>
        public static readonly AdLocation IAPStore = new("IAP Store");

        /// <summary>
        /// The store where a player buys virtual goods.
        /// </summary>
        public static readonly AdLocation ItemStore = new("Item Store");

        /// <summary>
        /// The game over screen after a player is finished playing.
        /// </summary>
        public static readonly AdLocation GameOver = new("Game Over");

        /// <summary>
        /// List of leaders in the game.
        /// </summary>
        public static readonly AdLocation LeaderBoard = new("Leaderboard");

        /// <summary>
        /// Screen where player can change settings such as sound.
        /// </summary>
        public static readonly AdLocation Settings = new("Settings");

        /// <summary>
        /// Screen display right before the player exists an app.
        /// </summary>
        public static readonly AdLocation Quit = new("Quit");

        public static AdLocation LocationFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Default;
            else if (map[name] != null)
                return map[name] as AdLocation;
            else
                return new(name);
        }
    }

#pragma warning disable 0618
    [Obsolete]
    public static class AdLocationExtension
    {
        public static AdPlacement ToAdPlacement(this AdLocation location)
        {
            if (location == AdLocation.Default)
                return AdPlacement.Default;
            else
                return AdPlacement.PlacementWithName(location.ToString());
        }
    }
#pragma warning restore 0618
}