using System;

namespace JamSabana.Core
{
    /// <summary>
    /// Canal de comunicación estático (Publisher-Subscriber) que contiene los eventos de las tareas del Programador B.
    /// Permite conectar sistemas como el de asimilación, mundo, HUD y ciclo de juego de forma totalmente desacoplada.
    /// </summary>
    public static class GameEventsB
    {

        /// <summary>
        /// Se dispara cuando una zona del escenario cambia de dueño.
        /// Recibe el equipo nuevo de la zona, el ID de la zona y el valor de progreso que esta aporta a la asimilación.
        /// </summary>
        public static event Action<PlayerTeam, int, float> OnWorldZoneConverted;
        public static void TriggerWorldZoneConverted(PlayerTeam team, int zoneId, float progressAmount) => OnWorldZoneConverted?.Invoke(team, zoneId, progressAmount);

        /// <summary>
        /// Se dispara cada vez que cambia el balance de asimilación unificado del juego.
        /// Envía el nuevo valor del balance (rango 0.0f a 1.0f, donde 0.5f es el centro de la barra).
        /// </summary>
        public static event Action<float> OnAssimilationChanged;
        public static void TriggerAssimilationChanged(float balance) => OnAssimilationChanged?.Invoke(balance);

        /// <summary>
        /// Se dispara cuando el mundo de una de las facciones es totalmente asimilado por el rival.
        /// Recibe la facción que ha sido derrotada (cuyo mundo llegó al 100% de asimilación enemiga).
        /// </summary>
        public static event Action<PlayerTeam> OnWorldFullyAssimilated;
        public static void TriggerWorldFullyAssimilated(PlayerTeam losingTeam) => OnWorldFullyAssimilated?.Invoke(losingTeam);

        /// <summary>
        /// Se dispara cuando se declara el fin de la partida de forma oficial.
        /// Recibe el número del jugador que ganó (1 o 2).
        /// </summary>
        public static event Action<int> OnMatchEnded;
        public static void TriggerMatchEnded(int winnerPlayer) => OnMatchEnded?.Invoke(winnerPlayer);

        /// <summary>
        /// Se dispara cuando un jugador construye o consume una batería de poder.
        /// Recibe el ID del jugador (1 o 2) y si actualmente posee la batería lista para usar.
        /// </summary>
        public static event Action<int, bool> OnBatteryCompleted;
        public static void TriggerBatteryCompleted(int playerId, bool hasBattery) => OnBatteryCompleted?.Invoke(playerId, hasBattery);

        /// <summary>
        /// Se dispara cuando el jugador obtiene o gasta un poder de ataque.
        /// Parámetros: int (ID del jugador: 1 o 2), PowerType (tipo de poder obtenido, o PowerType.None si se consume)
        /// </summary>
        public static event Action<int, PowerType> OnPowerObtained;
        public static void TriggerPowerObtained(int playerId, PowerType powerType) => OnPowerObtained?.Invoke(playerId, powerType);

        /// <summary>
        /// Se dispara cuando un jugador recoge o consume una venda de reparación.
        /// Recibe el ID del jugador (1 o 2) y si tiene al menos una venda disponible en su inventario rápido.
        /// </summary>
        public static event Action<int, bool> OnBandageAdded;
        public static void TriggerBandageAdded(int playerId, bool hasBandage) => OnBandageAdded?.Invoke(playerId, hasBandage);

        /// <summary>
        /// Se dispara cuando cambia la cantidad de partes de batería recolectadas por el jugador.
        /// Parámetros: int (ID del jugador: 1 o 2), int (cantidad actual de piezas)
        /// </summary>
        public static event Action<int, int> OnBatteryPartChanged;
        public static void TriggerBatteryPartChanged(int playerId, int amount) => OnBatteryPartChanged?.Invoke(playerId, amount);
    }
}
