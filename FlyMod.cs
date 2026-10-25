using System.Collections.Generic;
using BoneLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FlyMod.Main), "FlyMod", "1.0.0", "you")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace FlyMod
{
    public class Main : MelonMod
    {
        // Controls (change if you like):
        //   Left thumbstick click  -> toggle fly on/off  (see ToggleDown below)
        //   Left stick             -> move in the direction you are LOOKING
        //   Right stick up/down    -> go straight up / down
        //   Right grip held        -> speed boost
        private const float BaseSpeed = 6f;
        private const float BoostMultiplier = 3f;
        private const float VerticalSpeed = 5f;

        private bool _flying;
        private bool _lastToggle;
        private Rigidbody[] _bodies;
        private Dictionary<Rigidbody, bool> _origGravity = new Dictionary<Rigidbody, bool>();

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("FlyMod loaded. Click left stick to toggle flight.");
        }

        public override void OnLevelInitialized(int buildIndex)
        {
            _flying = false;
            _bodies = null;
            _origGravity.Clear();
        }

        public override void OnUpdate()
        {
            var rig = Player.PhysicsRig;
            if (rig == null) return;

            // --- toggle ---
            bool toggleNow = Player.LeftController != null && Player.LeftController.GetThumbStick();
            if (toggleNow && !_lastToggle) SetFlying(rig, !_flying);
            _lastToggle = toggleNow;

            if (!_flying) return;

            var head = Player.Head;
            if (head == null) return;

            Vector2 move = Player.LeftController.GetThumbStickAxis();
            Vector2 rightStick = Player.RightController.GetThumbStickAxis();
            bool boost = Player.RightController.GetGrabbedState();

            float speed = BaseSpeed * (boost ? BoostMultiplier : 1f);

            Vector3 dir = head.forward * move.y + head.right * move.x;
            Vector3 velocity = dir * speed + Vector3.up * (rightStick.y * VerticalSpeed * (boost ? BoostMultiplier : 1f));

            // Set every body part of the avatar to the same velocity so the whole rig moves together
            foreach (var rb in _bodies)
            {
                if (rb == null) continue;
                rb.velocity = velocity;
            }
        }

        private void SetFlying(Il2CppSLZ.Marrow.PhysicsRig rig, bool on)
        {
            _flying = on;
            if (on)
            {
                _bodies = rig.GetComponentsInChildren<Rigidbody>();
                _origGravity.Clear();
                foreach (var rb in _bodies)
                {
                    _origGravity[rb] = rb.useGravity;
                    rb.useGravity = false;
                }
            }
            else if (_bodies != null)
            {
                foreach (var rb in _bodies)
                {
                    if (rb == null) continue;
                    rb.useGravity = _origGravity.TryGetValue(rb, out var g) ? g : true;
                }
            }
            LoggerInstance.Msg(on ? "Fly ON" : "Fly OFF");
        }
    }
}
