using System;
using System.Collections.Generic;
using System.Media;

namespace EdcHost;

/// <summary>
/// A game
/// </summary>
public class Game
{
    #region Parameters.

    #region Parameters related to the game and the scoring machanism.

    /// <summary>
    /// The game duration of each game part.
    /// </summary>
    public static readonly Dictionary<GameStageType, long?> GameDuration =
        new Dictionary<GameStageType, long?> {
            { GameStageType.FirstHalf, 60000 },
            { GameStageType.SecondHalf, 180000 },
            { GameStageType.PreMatch, null }
        };

    /// <summary>
    /// The score obtained when having moved into the inner court.
    /// </summary>
    public const decimal ScoreMoveIntoInnerCourt = 10M;

    /// <summary>
    /// The range of the scores obtained when having an order delivered.
    /// </summary>
    public static readonly (decimal Lower, decimal Upper) ScoreDeliverOrderRange = (10M, 50M);

    /// <summary>
    /// The score obtained when a delivery is over time per
    /// millisecond.
    /// </summary>
    public const decimal ScoreDeliveryOvertimeRate = -0.005M;

    /// <summary>
    /// The score obtained when a vehicle is hitting a wall
    /// per millisecond.
    /// </summary>
    public const decimal ScoreHittingWallRate = -0.01M;

    /// <summary>
    /// The score obtained when the vehicle park overtime per
    /// millisecond.
    /// </summary>
    public const decimal ScoreOvertimeParkingRate = -0.005M;

    /// <summary>
    /// The score obtained when setting a charging pile.
    /// </summary>
    public const decimal ScoreSetChargingPile = 5M;

    /// <summary>
    /// The score obtained when gaining a foul flag.
    /// </summary>
    public const decimal ScoreFoul = -50M;

    #endregion

    #region Parameters related to the court.

    /// <summary>
    /// The court area.
    /// </summary>
    public static readonly (Dot TopLeft, Dot BottomRight) CourtArea = (
        new Dot(0, 0),
        new Dot(254, 254)
    );

    /// <summary>
    /// The inner court area.
    /// </summary>
    public static readonly (Dot TopLeft, Dot BottomRight) InnerCourtArea = (
        new Dot(40, 40),
        new Dot(214, 214)
    );

    /// <summary>
    /// The walls.
    /// </summary>
    public static readonly Barrier[] WallList = {
        // Walls on the top.
        new Barrier(new Dot(38, 38), new Dot(107, 40)),
        new Barrier(new Dot(147, 38), new Dot(216, 40)),
        // Walls on the bottom.
        new Barrier(new Dot(38, 214), new Dot(107, 216)),
        new Barrier(new Dot(147, 214), new Dot(216, 216)),
        // Walls on the left.
        new Barrier(new Dot(38, 38), new Dot(40, 107)),
        new Barrier(new Dot(38, 147), new Dot(40, 216)),
        // Walls on the right.
        new Barrier(new Dot(214, 38), new Dot(216, 107)),
        new Barrier(new Dot(214, 147), new Dot(216, 216))
    };

    #endregion

    #region Parameters related to the barriers.

    /// <summary>
    /// The range of the areas of the barriers.
    /// </summary>
    public static readonly (int Min, int Max) BarrierAreaRange = (
        250, 2500
    );

    /// <summary>
    /// The number of the barriers.
    /// </summary>
    public const int BarrierNumber = 5;

    /// <summary>
    /// The range of the side lengths of the barriers.
    /// </summary>
    public static readonly (int Min, int Max) BarrierSideLengthRange = (
        10, 170
    );

    /// <summary>
    /// The reduction rate of the max distances of vehicles in barriers
    /// in centimeter per millisecond.
    /// </summary>
    public const decimal BarrierDischargingRate = -0.1M;

    #endregion

    #region Parameters related to the charging piles.

    /// <summary>
    /// The max number of charging piles of a camp.
    /// </summary>
    public const int ChargingPileMaxNumber = 3;

    /// <summary>
    /// The increasing rate of the max distances of vehicles in the influence
    /// scope of their own charging piles in centimeter per second.
    /// </summary>
    public const decimal ChargingPileChargingRate = 1M;

    /// <summary>
    /// The reduction rate of the max distances of vehicles in the influence
    /// scope of their opponents' charging piles in centimeter per second.
    /// </summary>
    public const decimal ChargingPileDischargingRate = -0.1M;

    /// <summary>
    /// The radius of the scope where vehicles are influenced by
    /// charging piles.
    /// </summary>
    public const decimal ChargingPileInfluenceScopeRadius = 20M;

    #endregion

    #region Parameters related to orders.

    /// <summary>
    /// The radius of the scope where vehicles can contact a order.
    /// </summary>
    public const decimal OrderContactScopeRadius = 8M;

    /// <summary>
    /// The capacity of the orders on a vehicle.
    /// </summary>
    public const int OrderDeliveryCapacity = 5;

    /// <summary>
    /// The range of the delivery durations of orders.
    /// </summary>
    public static readonly (long Min, long Max) OrderDeliveryDurationRange = (
        20000, 60000
    );

    public const decimal OrderForceContactScopeRadius = 16M;

    /// <summary>
    /// The order number of each game part.
    /// </summary>
    public static readonly Dictionary<GameStageType, int?> OrderNumber =
        new Dictionary<GameStageType, int?>
        {
            { GameStageType.FirstHalf, 10 },
            { GameStageType.SecondHalf, 60 },
            { GameStageType.PreMatch, null }
        };

    #endregion

    #region Parameters related to vehicles

    /// <summary>
    /// The initial max distance of vehicles.
    /// </summary>
    public const int VehicleInitialMaxDistance = 4000;

    /// <summary>
    /// The increasing rate of the max distances of vehicles out of power
    /// in centimeter per millisecond.
    /// </summary>
    public const decimal VehicleAutoChargingRate = 0.02M;

    /// <summary>
    /// The step in milliseconds of auto charging.
    /// </summary>
    public const long VehicleAutoChargingStep = 5000;

    #endregion

    #endregion


    #region Public properties

    /// <summary>
    /// A list of the barriers.
    /// </summary>
    public List<Barrier> BarrierList => this._barrierList;

    /// <summary>
    /// The camp of the current vehicle. Null if the game has not 
    /// started.
    /// </summary>
    public CampType? Camp => this._camp;

    /// <summary>
    /// A list of the charing piles.
    /// </summary>
    public List<ChargingPile> ChargingPileList => this._chargingPileList;

    /// <summary>
    /// The game stage.
    /// </summary>
    public GameStageType GameStage => this._gameStage;

    /// <summary>
    /// The game state.
    /// </summary>
    public GameStatusType GameState => this._gameState;

    /// <summary>
    /// The game time. Null if the game has not started.
    /// </summary>
    public long? GameTime
    {
        get
        {
            if (this._startTime == null)
            {
                return null;
            }

            return (Utility.SystemTime - (long)this._startTime);
        }
    }

    /// <summary>
    /// The duration of the last tick
    /// </summary>
    /// <remarks>
    /// Should be update externally.
    /// </remarks>
    public long LastTickDuration => this._lastTickDuraion;

    /// <summary>
    /// The time of the last tick
    /// </summary>
    /// <remarks>
    /// Should be update externally.
    /// </remarks>
    public long LastTickTime => this._lastTickTime;

    /// <summary>
    /// A list of the generated orders.
    /// </summary>
    public List<Order> OrderList => this._orderList;

    /// <summary>
    /// The remaining time of the game. Null if the game has not
    /// started.
    /// </summary>
    public long? RemainingTime
    {
        get
        {
            if (this.GameTime == null)
            {
                return null;
            }

            if (Game.GameDuration[this._gameStage] == null)
            {
                return null;
            }

            return Math.Max((long)Game.GameDuration[this._gameStage] - (long)this.GameTime, 0);
        }
    }

    /// <summary>
    /// The scores of different camps.
    /// </summary>
    public Dictionary<CampType, decimal> Score => this._score;

    /// <summary>
    /// The vehicles of different camps.
    /// </summary>
    public Dictionary<CampType, Vehicle> Vehicle => this._vehicle;

    #endregion


    #region Private properties and fields

    private static readonly SoundPlayer _soundDeliverOrder = new SoundPlayer(
    @"Assets/Sounds/Deliver.wav"
);

    private static readonly SoundPlayer _soundTakeOrder = new SoundPlayer(
        @"Assets/Sounds/Order.wav"
    );

    private static readonly SoundPlayer _soundNotMoving = new SoundPlayer(
        @"Assets/Sounds/NotMoving.wav"
    );
    private static bool _isSoundNotMovingPlaying = false;

    private static readonly SoundPlayer _soundInBarrier = new SoundPlayer(
        @"Assets/Sounds/InBarrier.wav"
    );
    private static bool _isSoundInBarrierPlaying = false;

    private static readonly SoundPlayer _soundCharging = new SoundPlayer(
        @"Assets/Sounds/Charging.wav"
    );
    private static bool _isSoundChargingPlaying = false;

    private static readonly SoundPlayer _soundDischarging = new SoundPlayer(
        @"Assets/Sounds/InBarrier.wav"
    );
    private static bool _isSoundDischargingPlaying = false;

    private static readonly SoundPlayer _soundAutoCharge = new SoundPlayer(
        @"Assets/Sounds/AutoCharge.wav"
    );

    private static readonly SoundPlayer _soundSetChargingPile = new SoundPlayer(
        @"Assets/Sounds/SetChargingPile.wav"
    );

    private readonly List<Barrier> _barrierList;

    private CampType? _camp = null;

    private List<ChargingPile> _chargingPileList = new List<ChargingPile>();

    private GameStageType _gameStage = GameStageType.PreMatch;

    private GameStatusType _gameState = GameStatusType.Unstarted;

    // True if the vehicle of the current camp has moved into the
    // inner court.
    private bool? _hasMovedIntoInnerCourt = null;

    private long _lastTickDuraion = 1000; // Just a magic default value.

    // Minus one to prevent division by zero.
    private long _lastTickTime = Utility.SystemTime - 1;

    private OrderGenerator _orderGenerator = null;

    private List<Order> _orderList = new List<Order>();

    private long? _pauseTime = null;

    private Dictionary<CampType, decimal> _score =
        new Dictionary<CampType, decimal>
        {
            { CampType.A, 0M },
            { CampType.B, 0M }
        };

    private long? _startTime = null;

    private Dictionary<CampType, Vehicle> _vehicle = null;

    #endregion


    #region Public methods

    /// <summary>
    /// Construct a Game object.
    /// </summary>
    public Game()
    {
        // Load sounds
        Game._soundDeliverOrder.Load();
        Game._soundTakeOrder.Load();
        Game._soundNotMoving.Load();
        Game._soundInBarrier.Load();
        Game._soundCharging.Load();
        Game._soundDischarging.Load();
        Game._soundAutoCharge.Load();
        Game._soundSetChargingPile.Load();

        // Generate barriers
        this._barrierList = new List<Barrier>();

        for (int i = 0; i < Game.BarrierNumber; ++i)
        {
            bool isGenerated = false;
            while (!isGenerated)
            {
                var width = Utility.RandomGenerator.Next(
                    Game.BarrierSideLengthRange.Min,
                    Game.BarrierSideLengthRange.Max
                );
                var height = Utility.RandomGenerator.Next(
                    Game.BarrierSideLengthRange.Min,
                    Game.BarrierSideLengthRange.Max
                );

                // Restrict the area of the barriers.
                if (
                    width * height < Game.BarrierAreaRange.Min ||
                    width * height > Game.BarrierAreaRange.Max
                )
                {
                    continue;
                }

                var x = Utility.RandomGenerator.Next(
                    Game.InnerCourtArea.TopLeft.X,
                    Game.InnerCourtArea.BottomRight.X - width
                );
                var y = Utility.RandomGenerator.Next(
                    Game.InnerCourtArea.TopLeft.Y,
                    Game.InnerCourtArea.BottomRight.Y - height
                );

                this._barrierList.Add(new Barrier(
                    new Dot(x, y), new Dot(x + width, y + height)
                ));

                isGenerated = true;
            }
        }

    }

    /// <summary>
    /// Refresh the game.
    /// </summary>
    public void Refresh()
    {
        this._lastTickDuraion = Utility.SystemTime - this._lastTickTime;
        this._lastTickTime = Utility.SystemTime;

        // The game should only refresh when running.
        if (
            this._gameState != GameStatusType.Running
        )
        {
            return;
        }

        // Validate the fields.
        if (this._camp == null)
        {
            throw new Exception("The camp is invalid.");
        }
        if (this._startTime == null)
        {
            throw new Exception("The start time is not recorded.");
        }
        if (this._gameStage == GameStageType.PreMatch)
        {
            throw new Exception("The game runs in pre-match stage.");
        }

        // End the game if the time is up.
        if ((long)this.RemainingTime <= 0)
        {
            this.End();
        }

        this.TakeAndDeliverOrder();

        this.ScoreMoving();

        this.ScoreHittingWall();

        this.TackleBarriers();

        this.TackleChargingPiles();

        this.AutoCharge();

        // Must generate orders after refreshing the game to avoid interacting with
        // orders not processed by the slave.
        this.GenerateOrder();
    }

    /// <summary>
    /// Start a part of the game.
    /// </summary>
    /// <param name="camp">The camp participating in the game.</param>
    /// <param name="gameStage">The game stage.</param>
    public void Start(CampType camp, GameStageType gameStage)
    {
        if (
            this._gameState != GameStatusType.Unstarted &&
            this._gameState != GameStatusType.Ended
        )
        {
            throw new Exception("The game has started.");
        }

        // Validate the parameters
        if (gameStage == GameStageType.PreMatch)
        {
            throw new Exception("The game stage is invalid.");
        }

        this._gameState = GameStatusType.Running;

        // Set the metadata.
        this._camp = camp;
        this._gameStage = gameStage;

        // Set the vehicles.
        this._vehicle = new Dictionary<CampType, Vehicle>
        {
            { CampType.A, new Vehicle(CampType.A, initialMaxDistance: Game.VehicleInitialMaxDistance) },
            { CampType.B, new Vehicle(CampType.B, initialMaxDistance: Game.VehicleInitialMaxDistance) }
        };

        // Set the start time
        this._startTime = Utility.SystemTime;

        // Set the order generator.
        switch (this._camp)
        {
            case CampType.A:
                this._orderList.Clear();
                this._orderGenerator = new OrderGenerator(
                    count: (int)Game.OrderNumber[this._gameStage],
                    area: Game.CourtArea,
                    generationTimeRange: (0, (long)Game.GameDuration[this._gameStage]),
                    timeLimitRange: Game.OrderDeliveryDurationRange,
                    commissionRange: Game.ScoreDeliverOrderRange,
                    barrierList: this._barrierList
                );
                break;

            case CampType.B:
                this._orderList.Clear();
                if (this._orderGenerator == null)
                {
                    throw new Exception("The order generator is null.");
                }
                this._orderGenerator.Reset();
                break;

            default:
                throw new Exception("The camp is invalid.");
        }

        // Set other things.
        this._hasMovedIntoInnerCourt = false;
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void Pause()
    {
        if (this._gameState != GameStatusType.Running)
        {
            throw new Exception("The game is not running.");
        }

        this._gameState = GameStatusType.Paused;

        // Record the time when start to pause.
        this._pauseTime = Utility.SystemTime;
    }

    /// <summary>
    /// Continue the game.
    /// </summary>
    public void Continue()
    {
        if (this._gameState != GameStatusType.Paused)
        {
            throw new Exception("The game is not paused.");
        }

        this._gameState = GameStatusType.Running;

        // To reduce the paused time in the game time.
        this._startTime += Utility.SystemTime - this._pauseTime;
    }

    /// <summary>
    /// End the game.
    /// </summary>
    public void End()
    {
        if (
            this._gameState != GameStatusType.Running &&
            this._gameState != GameStatusType.Paused
        )
        {
            throw new Exception("The game is not running or paused.");
        }

        Game._soundDeliverOrder.Stop();
        Game._soundTakeOrder.Stop();
        Game._soundNotMoving.Stop();
        Game._soundInBarrier.Stop();
        Game._soundCharging.Stop();
        Game._soundDischarging.Stop();
        Game._soundAutoCharge.Stop();
        Game._soundSetChargingPile.Stop();

        this._gameState = GameStatusType.Ended;
    }

    /// <summary>
    /// Force to take or deliver an order.
    /// </summary>
    public void ForceToTakeOrDeliverOrder(ForceToTakeOrDeliverOrderType action)
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle?.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;


        // Count the orders in delivery.
        var deliveringOrderNumber = 0;
        foreach (var order in this._orderList)
        {
            if (order.Status == OrderStatusType.InDelivery)
            {
                ++deliveringOrderNumber;
            }
        }

        foreach (var order in this._orderList)
        {
            // Take orders.
            if (order.Status == OrderStatusType.Pending && action == ForceToTakeOrDeliverOrderType.Take)
            {
                // Check if the capacity is full.
                if (deliveringOrderNumber >= Game.OrderDeliveryCapacity)
                {
                    continue;
                }

                if ((decimal)Dot.Distance(order.DeparturePosition, vehiclePosition)
                    <= Game.OrderForceContactScopeRadius)
                {
                    order.Take((long)this.GameTime);

                    // Play the take sound.
                    Game._soundTakeOrder.Play();
                }
            }
            else if (order.Status == OrderStatusType.InDelivery)
            {
                if ((decimal)Dot.Distance(order.DestinationPosition, vehiclePosition)
                    <= Game.OrderForceContactScopeRadius)
                {
                    order.Deliver((long)this.GameTime);

                    if (order.OvertimeDuration == null)
                    {
                        throw new Exception("The overtime duration of the order is null.");
                    }

                    this._score[(CampType)this._camp] +=
                        Math.Max((decimal)order.Commission + Game.ScoreDeliveryOvertimeRate * (byte)order.OvertimeDuration, 0);

                    // Player the deliver sound.
                    Game._soundDeliverOrder.Play();
                }
            }
        }
    }

    /// <summary>
    /// Set a charging pile.
    /// </summary>
    public void SetChargingPile()
    {
        if (this._camp == null)
        {
            throw new Exception("The camp is invalid.");
        }

        if (this._vehicle[(CampType)this._camp].Position == null)
        {
            return;
        }

        // Avoid setting too many charging piles.
        int chargingPileNumber = 0;
        foreach (var chargingPile in this._chargingPileList)
        {
            if (chargingPile.Camp == this._camp)
            {
                ++chargingPileNumber;
            }
        }
        if (chargingPileNumber >= Game.ChargingPileMaxNumber)
        {
            return;
        }

        // Can only set charging piles in the first half.
        if (this._gameStage != GameStageType.FirstHalf)
        {
            return;
        }

        this._chargingPileList.Add(new ChargingPile(
            (CampType)this._camp,
            (Dot)this._vehicle[(CampType)this._camp].Position,
            influenceScopeRadius: Game.ChargingPileInfluenceScopeRadius
        ));

        this._score[(CampType)this._camp] += Game.ScoreSetChargingPile;

        Game._soundSetChargingPile.Play();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Auto charge if the power of the vehicle is used up.
    /// </summary>
    private void AutoCharge()
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        if (vehicle.IsPowerExhausted)
        {
            Game._soundAutoCharge.Play();

            // Exchange time for power.
            this._startTime -= Game.VehicleAutoChargingStep;
            vehicle.IncreaseMaxDistance(
                (int)(Game.VehicleAutoChargingRate * Game.VehicleAutoChargingStep)
            );
        }
    }

    /// <summary>
    /// Attempt to generate an order.
    /// </summary>
    private void GenerateOrder()
    {
        var newOrder = this._orderGenerator.Generate((long)this.GameTime);
        if (newOrder != null)
        {
            this._orderList.Add(newOrder);
        }
    }

    /// <summary>
    /// Check if a position is in barrier area.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>
    /// True if the poisition is in barrier area; otherwise false.
    /// </returns>
    private bool IsInBarrier(Dot position)
    {
        foreach (var barrier in this._barrierList)
        {
            if (barrier.IsIn(position))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if a position is in wall area.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>
    /// True if the poisition is in wall area; otherwise false.
    /// </returns>
    private bool IsInWall(Dot position)
    {
        foreach (var wall in Game.WallList)
        {
            if (wall.IsIn(position))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Check if a position is in the influence scope of charging
    /// piles of a camp.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="camp">The camp.</param>
    /// <param name="reverse">
    /// True if to check the position is in the influence scope of 
    /// charging piles of other camps.
    /// </param>
    /// <returns>
    /// True if the position is in the influence scope; otherwise false.
    /// </returns>
    private bool IsInChargingPileInfluenceScope(
        Dot position,
        CampType camp,
        bool reverse = false
    )
    {
        foreach (var chargingPile in this._chargingPileList)
        {
            if (
                (
                    (
                        chargingPile.Camp == camp && !reverse
                    ) ||
                    (
                        chargingPile.Camp != camp && reverse
                    )
                ) &&
                chargingPile.IsInInfluenceScope(position)
            )
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Judge if the vehicle is hitting the wall and score
    /// according to it.
    /// </summary>
    void ScoreHittingWall()
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;

        if (this.IsInWall(vehiclePosition))
        {
            this._score[(CampType)this._camp] +=
                Game.ScoreHittingWallRate * this.LastTickDuration;
        }
    }

    /// <summary>
    /// Judge the moving status of the vehicle and score
    /// according to it.
    /// </summary>
    void ScoreMoving()
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;

        // Score first moving into inner court.
        if (this._hasMovedIntoInnerCourt == null)
        {
            throw new Exception("_hasMovedIntoInnerCourt == null.");
        }

        if (
            !(bool)this._hasMovedIntoInnerCourt &&
            vehiclePosition.X >= Game.InnerCourtArea.TopLeft.X &&
            vehiclePosition.Y >= Game.InnerCourtArea.TopLeft.Y &&
            vehiclePosition.X <= Game.InnerCourtArea.BottomRight.X &&
            vehiclePosition.Y <= Game.InnerCourtArea.BottomRight.Y
        )
        {
            this._hasMovedIntoInnerCourt = true;
            this._score[(CampType)this._camp] += Game.ScoreMoveIntoInnerCourt;
        }

        // Score parking penalty.
        if (
            vehicle.ParkingDuration != null &&
            (long)vehicle.ParkingDuration >= 5000 + this.LastTickDuration
        )
        {
            if (!Game._isSoundNotMovingPlaying)
            {
                Game._isSoundNotMovingPlaying = true;
                Game._soundNotMoving.PlayLooping();
            }
            this._score[(CampType)this._camp] +=
                Game.ScoreOvertimeParkingRate * this.LastTickDuration;
        }
        else
        {
            if (Game._isSoundNotMovingPlaying)
            {
                Game._isSoundNotMovingPlaying = false;
                Game._soundNotMoving.Stop();
            }
        }
    }

    /// <summary>
    /// Tackle the moving on barriers status.
    /// </summary>
    void TackleBarriers()
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;

        if (this.IsInBarrier(vehiclePosition))
        {
            if (!Game._isSoundInBarrierPlaying)
            {
                Game._isSoundInBarrierPlaying = true;
                Game._soundInBarrier.PlayLooping();
            }

            vehicle.IncreaseMaxDistance(
                (int)Math.Round(Game.BarrierDischargingRate * this.LastTickDuration)
            );
        }
        else
        {
            if (Game._isSoundInBarrierPlaying)
            {
                Game._isSoundInBarrierPlaying = false;
                Game._soundInBarrier.Stop();
            }
        }
    }

    /// <summary>
    /// Tackle the charging and discharing status.
    /// </summary>
    void TackleChargingPiles()
    {
        if (this._gameStage != GameStageType.SecondHalf)
        {
            return;
        }

        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;

        if (this.IsInChargingPileInfluenceScope(
            position: vehiclePosition,
            camp: (CampType)this._camp
        ))
        {
            if (!Game._isSoundChargingPlaying)
            {
                Game._isSoundChargingPlaying = true;
                Game._soundCharging.PlayLooping();
            }

            vehicle.IncreaseMaxDistance(
                (int)Math.Round(Game.ChargingPileChargingRate * this.LastTickDuration)
            );
        }
        else
        {
            if (Game._isSoundChargingPlaying)
            {
                Game._isSoundChargingPlaying = false;
                Game._soundCharging.Stop();
            }
        }

        if (this.IsInChargingPileInfluenceScope(
            position: vehiclePosition,
            camp: (CampType)this._camp,
            reverse: true
        ))
        {
            if (!Game._isSoundDischargingPlaying)
            {
                Game._isSoundDischargingPlaying = true;
                Game._soundDischarging.PlayLooping();
            }

            vehicle.IncreaseMaxDistance(
                (int)Math.Round(Game.ChargingPileDischargingRate * this.LastTickDuration)
            );
        }
        else
        {
            if (Game._isSoundDischargingPlaying)
            {
                Game._isSoundDischargingPlaying = false;
                Game._soundDischarging.Stop();
            }
        }
    }

    /// <summary>
    /// Take and deliver orders.
    /// </summary>
    void TakeAndDeliverOrder()
    {
        var vehicle = this._vehicle[(CampType)this._camp];

        if (vehicle.Position == null)
        {
            return;
        }

        var vehiclePosition = (Dot)vehicle.Position;


        // Count the orders in delivery.
        var deliveringOrderNumber = 0;
        foreach (var order in this._orderList)
        {
            if (order.Status == OrderStatusType.InDelivery)
            {
                ++deliveringOrderNumber;
            }
        }

        foreach (var order in this._orderList)
        {
            // Take orders.
            if (order.Status == OrderStatusType.Pending)
            {
                // Check if the capacity is full.
                if (deliveringOrderNumber >= Game.OrderDeliveryCapacity)
                {
                    continue;
                }

                if ((decimal)Dot.Distance(order.DeparturePosition, vehiclePosition)
                    <= Game.OrderContactScopeRadius)
                {
                    order.Take((long)this.GameTime);

                    // Play the take sound.
                    Game._soundTakeOrder.Play();
                }
            }
            else if (order.Status == OrderStatusType.InDelivery)
            {
                if ((decimal)Dot.Distance(order.DestinationPosition, vehiclePosition)
                    <= Game.OrderContactScopeRadius)
                {
                    order.Deliver((long)this.GameTime);

                    if (order.OvertimeDuration == null)
                    {
                        throw new Exception("The overtime duration of the order is null.");
                    }

                    this._score[(CampType)this._camp] +=
                        Math.Max((decimal)order.Commission + Game.ScoreDeliveryOvertimeRate * (long)order.OvertimeDuration, 0);

                    // Player the deliver sound.
                    Game._soundDeliverOrder.Play();
                }
            }
        }
    }

    /// <summary>
    /// Set a foul flag.
    /// </summary>
    public void SetFoul()
    {
        this._score[(CampType)this._camp] += Game.ScoreFoul;
    }

    #endregion
}