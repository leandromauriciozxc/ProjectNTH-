using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectNTH.Elevators
{
    /// <summary>A stationary cabin: doors and indicator simulate travel between editable floors.</summary>
    [DisallowMultipleComponent]
    public sealed class ElevatorController : MonoBehaviour
    {
        public enum RideState { Idle, Closing, Travelling, Arriving, Opening, Starting }

        [Serializable]
        public sealed class Floor
        {
            public string label = "1";
            [Tooltip("Runs once at this destination, with the doors fully closed. Use this to switch the surrounding area.")]
            public UnityEvent onArrival = new UnityEvent();
        }

        [Header("Doors (place in their CLOSED positions in Edit Mode)")]
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;
        [Tooltip("Offset in the door parent's local space, not the scaled door's own space.")]
        [SerializeField] private Vector3 leftOpenOffset = Vector3.left * 1.25f;
        [SerializeField] private Vector3 rightOpenOffset = Vector3.right * 1.25f;
        [Min(0.01f), SerializeField] private float doorDuration = 1.5f;
        [Header("Play Mode startup")]
        [Tooltip("Start closed, wait Initial Opening Delay, then open using Door Duration.")]
        [SerializeField] private bool startOpen = true;
        [Min(0f), SerializeField] private float initialOpeningDelay;
        [Tooltip("Apply the starting floor's On Arrival event before opening. Enable when these events switch landing areas.")]
        [SerializeField] private bool applyStartingFloorOnStart;

        [Header("Floors (lowest to highest; indices begin at 0)")]
        [SerializeField] private Floor[] floors = { new Floor { label = "1" }, new Floor { label = "2" } };
        [Min(0), SerializeField] private int startingFloor;
        [Min(0.1f), SerializeField] private float secondsPerFloor = 2f;
        [Min(0f), SerializeField] private float arrivalPause = 0.6f;

        [Header("Hallway calls")]
        [Tooltip("Time to board after opening, before serving another queued hallway call.")]
        [Min(0.1f), SerializeField] private float hallwayBoardingTime = 3f;

        [Header("Optional sound (use two separate AudioSources)")]
        [SerializeField] private AudioSource effectsSource;
        [SerializeField] private AudioSource travelSource;
        [SerializeField] private AudioClip doorClip;
        [SerializeField] private AudioClip travelLoop;
        [SerializeField] private AudioClip arrivalChime;

        public event Action DisplayChanged;
        public RideState State { get; private set; }
        public int CurrentFloorIndex { get; private set; }
        public int TargetFloorIndex { get; private set; }
        public int FloorCount => floors == null ? 0 : floors.Length;
        public bool IsBusy => State != RideState.Idle;
        public int Direction => State == RideState.Travelling ? travelDirection : 0;
        public bool CanOperateDoors => initialized && isActiveAndEnabled
            && State != RideState.Travelling && State != RideState.Arriving && State != RideState.Starting;

        private Vector3 leftClosedPosition;
        private Vector3 rightClosedPosition;
        private int settledFloor;
        private int travelDirection;
        private float doorOpenness;
        private bool initialized;
        private Coroutine activeSequence;
        private bool startupComplete;
        private readonly Queue<int> hallwayQueue = new Queue<int>();
        private byte[] hallwayRequests;
        private float nextHallwayServiceTime;

        private void Awake()
        {
            if (leftDoor == null || rightDoor == null || leftDoor == rightDoor || FloorCount == 0)
            {
                Debug.LogError("Elevator needs two different doors and at least one floor.", this);
                enabled = false;
                return;
            }

            leftClosedPosition = leftDoor.localPosition;
            rightClosedPosition = rightDoor.localPosition;
            settledFloor = Mathf.Clamp(startingFloor, 0, FloorCount - 1);
            CurrentFloorIndex = TargetFloorIndex = settledFloor;
            hallwayRequests = new byte[FloorCount];
            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized) return;
            CurrentFloorIndex = TargetFloorIndex = settledFloor;
            if (!startupComplete)
            {
                SetDoorOpenness(0f);
                StartSequence(Startup());
                return;
            }
            SetDoorOpenness(startOpen ? 1f : 0f);
            SetState(RideState.Idle);
        }

        private IEnumerator Startup()
        {
            SetState(RideState.Starting);
            // Let all scene objects finish Awake before invoking scene-wiring events.
            yield return null;
            if (applyStartingFloorOnStart) floors[settledFloor]?.onArrival?.Invoke();
            if (!isActiveAndEnabled) yield break;
            if (startOpen && initialOpeningDelay > 0f) yield return new WaitForSeconds(initialOpeningDelay);
            // Manual door commands may reverse this opening without leaving startup unfinished.
            startupComplete = true;
            if (startOpen) yield return MoveDoorsAndIdle(true);
            else SetState(RideState.Idle);
        }

        private void Update()
        {
            if (initialized && startupComplete && !IsBusy && hallwayQueue.Count > 0
                && Time.time >= nextHallwayServiceTime) ServeNextHallwayCall();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            activeSequence = null;
            StopTravelSound();
            hallwayQueue.Clear();
            if (hallwayRequests != null) Array.Clear(hallwayRequests, 0, hallwayRequests.Length);
            if (!initialized) return;
            // An interrupted ride returns to the last destination whose arrival event ran.
            CurrentFloorIndex = TargetFloorIndex = settledFloor;
            State = RideState.Idle;
            DisplayChanged?.Invoke();
        }

        public string GetFloorLabel(int index)
        {
            if (index < 0 || index >= FloorCount) return "?";
            return floors[index] == null || string.IsNullOrWhiteSpace(floors[index].label)
                ? (index + 1).ToString() : floors[index].label;
        }

        /// <summary>Callable from a UnityEvent with a zero-based floor index.</summary>
        public void SelectFloor(int index) { TrySelectFloor(index); }

        public bool TrySelectFloor(int index)
        {
            if (!initialized || !isActiveAndEnabled || IsBusy || index < 0 || index >= FloorCount)
                return false;

            if (index == settledFloor)
            {
                OpenDoors();
                return true;
            }

            TargetFloorIndex = index;
            StartSequence(Ride());
            return true;
        }

        public void OpenDoors()
        {
            if (!CanOperateDoors || State == RideState.Opening
                || (State == RideState.Idle && doorOpenness >= 1f)) return;
            // Reopening during departure cancels that trip before the cabin starts travelling.
            // The player selects the destination again when ready to leave.
            if (TargetFloorIndex != settledFloor && hallwayRequests[TargetFloorIndex] != 0
                && !hallwayQueue.Contains(TargetFloorIndex)) hallwayQueue.Enqueue(TargetFloorIndex);
            TargetFloorIndex = settledFloor;
            StartSequence(MoveDoorsAndIdle(true));
        }

        public void CloseDoors()
        {
            if (!CanOperateDoors || State == RideState.Closing
                || (State == RideState.Idle && doorOpenness <= 0f)) return;
            // Closing by itself never selects a floor or starts travel.
            TargetFloorIndex = settledFloor;
            StartSequence(MoveDoorsAndIdle(false));
        }

        public bool IsValidHallwayCall(int floorIndex, int direction)
        {
            return floorIndex >= 0 && floorIndex < FloorCount
                && (direction == 1 ? floorIndex < FloorCount - 1 : direction == -1 && floorIndex > 0);
        }

        public bool IsHallwayCallPending(int floorIndex, int direction)
        {
            return hallwayRequests != null && IsValidHallwayCall(floorIndex, direction)
                && (hallwayRequests[floorIndex] & (direction > 0 ? 1 : 2)) != 0;
        }

        /// <summary>Call to the caller's landing, not to the next floor. Direction is +1 (up) or -1 (down).</summary>
        public bool CallToFloor(int floorIndex, int direction)
        {
            if (!initialized || !isActiveAndEnabled || !IsValidHallwayCall(floorIndex, direction)) return false;
            if (floorIndex == settledFloor && CanOperateDoors)
            {
                nextHallwayServiceTime = Time.time + hallwayBoardingTime;
                OpenDoors();
                return true;
            }
            // A cabin already arriving here is about to open, so another stop is unnecessary.
            if (floorIndex == TargetFloorIndex && State == RideState.Arriving) return true;
            if (hallwayRequests[floorIndex] == 0) hallwayQueue.Enqueue(floorIndex);
            hallwayRequests[floorIndex] |= (byte)(direction > 0 ? 1 : 2);
            DisplayChanged?.Invoke();
            if (startupComplete && !IsBusy) ServeNextHallwayCall();
            return true;
        }

        private void ServeNextHallwayCall()
        {
            while (hallwayQueue.Count > 0)
            {
                int floorIndex = hallwayQueue.Dequeue();
                if (hallwayRequests[floorIndex] == 0) continue;
                if (floorIndex == settledFloor)
                {
                    hallwayRequests[floorIndex] = 0;
                    DisplayChanged?.Invoke();
                    nextHallwayServiceTime = Time.time + hallwayBoardingTime;
                    OpenDoors();
                }
                else
                {
                    TargetFloorIndex = floorIndex;
                    StartSequence(Ride());
                }
                return;
            }
        }

        private void StartSequence(IEnumerator sequence)
        {
            // Stop the owning coroutine, including its nested door animation, before reversing.
            if (activeSequence != null) StopCoroutine(activeSequence);
            activeSequence = StartCoroutine(sequence);
        }

        private IEnumerator Ride()
        {
            SetState(RideState.Closing);
            yield return MoveDoors(0f);
            travelDirection = TargetFloorIndex > CurrentFloorIndex ? 1 : -1;
            SetState(RideState.Travelling);
            if (travelSource != null && travelLoop != null)
            {
                travelSource.clip = travelLoop;
                travelSource.loop = true;
                travelSource.Play();
            }

            while (CurrentFloorIndex != TargetFloorIndex)
            {
                yield return new WaitForSeconds(Mathf.Max(0.1f, secondsPerFloor));
                CurrentFloorIndex += travelDirection;
                DisplayChanged?.Invoke();
            }

            StopTravelSound();
            hallwayRequests[CurrentFloorIndex] = 0;
            SetState(RideState.Arriving);
            settledFloor = CurrentFloorIndex;
            // Keep this cabin/controller active when switching external floor areas.
            floors[settledFloor]?.onArrival?.Invoke();
            if (!isActiveAndEnabled) yield break;
            PlayEffect(arrivalChime);
            yield return new WaitForSeconds(Mathf.Max(0f, arrivalPause));
            yield return MoveDoorsAndIdle(true);
        }
        
        private IEnumerator MoveDoorsAndIdle(bool open)
        {
            SetState(open ? RideState.Opening : RideState.Closing);
            yield return MoveDoors(open ? 1f : 0f);
            SetState(RideState.Idle);
        }

        private IEnumerator MoveDoors(float target)
        {
            float from = doorOpenness;
            if (Mathf.Approximately(from, target)) yield break;
            PlayEffect(doorClip);
            float duration = Mathf.Max(0.01f, doorDuration) * Mathf.Abs(target - from);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                SetDoorOpenness(Mathf.Lerp(from, target, t));
                yield return null;
            }
            SetDoorOpenness(target);
        }

        private void SetDoorOpenness(float amount)
        {
            doorOpenness = amount;
            leftDoor.localPosition = leftClosedPosition + leftOpenOffset * amount;
            rightDoor.localPosition = rightClosedPosition + rightOpenOffset * amount;
        }

        private void SetState(RideState state)
        {
            State = state;
            if (state == RideState.Idle)
                nextHallwayServiceTime = Time.time + (doorOpenness > 0f ? Mathf.Max(0.1f, hallwayBoardingTime) : 0f);
            DisplayChanged?.Invoke();
        }

        private void PlayEffect(AudioClip clip)
        {
            if (effectsSource != null && clip != null) effectsSource.PlayOneShot(clip);
        }

        private void StopTravelSound()
        {
            if (travelSource != null) travelSource.Stop();
        }
    }
}
