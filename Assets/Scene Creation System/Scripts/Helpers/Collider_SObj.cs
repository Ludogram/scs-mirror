using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dhs5.SceneCreation
{
    public class Collider_SObj : BaseSceneObject
    {
        [Flags]
        public enum Interactions
        {
            CollisionEnter = 1 << 0,
            CollisionStay = 1 << 1,
            CollisionExit = 1 << 2,
            TriggerEnter = 1 << 3,
            TriggerStay = 1 << 4,
            TriggerExit = 1 << 5,
        }

        #region Variables
        [Header("Collider")]
        [SerializeField] new protected Collider collider;
        [Tooltip("Mask deciding which layers to collide with")]
        [SerializeField] protected LayerMask layerMask;

        [Header("Use")]
        [Tooltip("If false, deactivate after a certain number of use")]
        [SerializeField] protected bool infiniteUse = true;
        [Tooltip("Number of uses before deactivating the collider\n--> Use Reload() to reset to the original number")]
        [SerializeField][Min(1)] protected int useNumber = 1;

        [Header("Interactions")]
        [SerializeField] protected Interactions interactions;

        [Header("Collision")]
        [SerializeField] protected List<SceneEvent<Collision>> onCollisionEnter;
        [Space(15f)]
        [SerializeField] protected List<SceneEvent<Collision>> onCollisionStay;
        [Space(15f)]
        [SerializeField] protected List<SceneEvent<Collision>> onCollisionExit;

        [Header("Trigger")]
        [SerializeField] protected List<SceneEvent<Collider>> onTriggerEnter;
        [Space(15f)]
        [SerializeField] protected List<SceneEvent<Collider>> onTriggerStay;
        [Space(15f)]
        [SerializeField] protected List<SceneEvent<Collider>> onTriggerExit;

        public bool InfiniteUse => infiniteUse;

        public bool DoesCollisionEnter => (interactions & Interactions.CollisionEnter) != 0;
        public bool DoesCollisionStay => (interactions & Interactions.CollisionStay) != 0;
        public bool DoesCollisionExit => (interactions & Interactions.CollisionExit) != 0;
        public bool DoesTriggerEnter => (interactions & Interactions.TriggerEnter) != 0;
        public bool DoesTriggerStay => (interactions & Interactions.TriggerStay) != 0;
        public bool DoesTriggerExit => (interactions & Interactions.TriggerExit) != 0;

        public override string DisplayName => "Collider Scene Object";

        #endregion

        #region BaseSceneObject Extension
        protected override void UpdateSceneVariables()
        {
            Setup(onCollisionEnter, onCollisionStay, onCollisionExit);
            Setup(onTriggerEnter, onTriggerStay, onTriggerExit);
        }
        protected override void RegisterSceneElements()
        {
            RegisterEvents(
                (nameof(onCollisionEnter), onCollisionEnter),
                (nameof(onCollisionStay), onCollisionStay),
                (nameof(onCollisionExit), onCollisionExit));
            RegisterEvents(
                (nameof(onTriggerEnter), onTriggerEnter),
                (nameof(onTriggerStay), onTriggerStay),
                (nameof(onTriggerExit), onTriggerExit));
        }
        protected override void OnSceneObjectAwake()
        {
            base.OnSceneObjectAwake();

            Reload();
        }
        #endregion

        #region Collision
        protected virtual bool CollisionValid(Collision collision)
        {
            return ((1 << collision.gameObject.layer) & layerMask) != 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (DoesCollisionEnter && CollisionValid(collision))
            {
                onCollisionEnter.Trigger(collision);
                Use();
            }
        }
        private void OnCollisionStay(Collision collision)
        {
            if (DoesCollisionStay && CollisionValid(collision))
            {
                onCollisionStay.Trigger(collision);
                Use();
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (DoesCollisionExit && CollisionValid(collision))
            { 
                onCollisionExit.Trigger(collision);
                Use();
            }
        }
        #endregion

        #region Trigger
        protected virtual bool TriggerValid(Collider collider)
        {
            return ((1 << collider.gameObject.layer) & layerMask) != 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (DoesTriggerEnter && TriggerValid(other))
            { 
                onTriggerEnter.Trigger(other);
                Use();
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (DoesTriggerStay && TriggerValid(other))
            { 
                onTriggerStay.Trigger(other);
                Use();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (DoesTriggerExit && TriggerValid(other))
            { 
                onTriggerExit.Trigger(other);
                Use();
            }
        }
        #endregion

        #region Collider Management
        protected override void OnSceneObjectValidate()
        {
            base.OnSceneObjectValidate();

            if (collider == null)
            {
                collider = GetComponent<Collider>();
            }
            if (collider)
            {
                collider.includeLayers = layerMask;
            }
        }
        #endregion

        #region Use Management
        private int useLeft;

        private void Reload()
        {
            if (infiniteUse) 
            {
                collider.enabled = true;
                return; 
            }

            useLeft = useNumber;
            collider.enabled = useLeft != 0;
        }
        private void Use()
        {
            if (infiniteUse) { return; }

            useLeft--;
            if (useLeft == 0) { collider.enabled = false; }
        }
        #endregion
    }
}
