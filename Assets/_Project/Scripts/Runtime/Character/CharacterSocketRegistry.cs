using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowSupply.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterSocketRegistry : MonoBehaviour
    {
        [Serializable]
        private sealed class SocketEntry
        {
            public CharacterSocket socket;
            public Transform transform;
        }

        [SerializeField] private List<SocketEntry> sockets =
            new List<SocketEntry>();

        public void Configure(
            CharacterSocket socket,
            Transform socketTransform
        )
        {
            foreach (SocketEntry entry in sockets)
            {
                if (entry.socket == socket)
                {
                    entry.transform = socketTransform;
                    return;
                }
            }

            sockets.Add(
                new SocketEntry
                {
                    socket = socket,
                    transform = socketTransform
                }
            );
        }

        public Transform GetSocket(
            CharacterSocket socket
        )
        {
            foreach (SocketEntry entry in sockets)
            {
                if (
                    entry.socket == socket &&
                    entry.transform != null
                )
                {
                    return entry.transform;
                }
            }

            return transform;
        }

        public GameObject Attach(
            CharacterSocket socket,
            GameObject prefab
        )
        {
            if (prefab == null)
            {
                return null;
            }

            Transform target =
                GetSocket(socket);

            GameObject instance =
                Instantiate(
                    prefab,
                    target
                );

            instance.transform.localPosition =
                Vector3.zero;

            instance.transform.localRotation =
                Quaternion.identity;

            instance.transform.localScale =
                Vector3.one;

            return instance;
        }
    }
}
