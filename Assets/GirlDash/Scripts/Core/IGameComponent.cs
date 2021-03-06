﻿using UnityEngine;
using System.Collections;

namespace GirlDash {
    public interface IGameComponent {
        // Calls when game ready.
        void GameReady();
        // Calls when game start.
        void GameStart();
        // Calls when game end,
        // since this game never wins, so just call this when the player is dead.
        void GameOver();
        // Calls when game reset.
        void GameReset();
    }
}