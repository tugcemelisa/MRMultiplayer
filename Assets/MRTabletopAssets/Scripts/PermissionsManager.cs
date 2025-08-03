using System;
using System.Collections.Generic;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif // UNITY_ANDROID
using UnityEngine.Events;

namespace UnityEngine.XR.Templates.MRTTabletopAssets
{
    /// <summary>
    /// Utility class to help define and manage Android device permissions and specify corresponding permission callbacks via <see cref="UnityEvent"/>.
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class PermissionsManager : MonoBehaviour
    {
        const string k_DefaultPermissionId = "com.oculus.permission.USE_SCENE";

        [SerializeField, Tooltip("Enables or disables the processing of permissions on Start. If disabled, permissions will not be processed until the ProcessPermissions method is called.")]
        private bool m_ProcessPermissionsOnStart = true;

        [SerializeField, Tooltip("The system permissions to request when this component Starts.")]
        List<PermissionRequestGroup> m_PermissionGroups = new List<PermissionRequestGroup>();

        /// <summary>
        /// Current platform permission group to process. This is determined during the <see cref="Awake"/> method using based on <see cref="XRPlatformUnderstanding"/>
        /// </summary>
        PermissionRequestGroup m_CurrentPlatformPermissionGroup = new PermissionRequestGroup();

        List<PermissionRequest> m_Persmissions = new List<PermissionRequest>();

        /// <summary>
        /// A group of permissions to request based on a specific platform.
        /// </summary>
        [Serializable]
        class PermissionRequestGroup
        {
            [HideInInspector]
            public string name;

            [Tooltip("The platform type for which these permissions is intended for.")]
            public XRPlatformType platformType;

            public List<PermissionRequest> permissions;
        }

        /// <summary>
        /// A permission request to be made to the Android operating system.
        /// </summary>
        [Serializable]
        class PermissionRequest
        {
            [Tooltip("The Android system permission to request when this component starts.")]
            public string permissionId = k_DefaultPermissionId;

            [Tooltip("Whether to request permission from the operating system.")]
            public bool enabled = true;

            [HideInInspector]
            public bool requested = false;

            [HideInInspector]
            public bool responseReceived = false;

            [HideInInspector]
            public bool granted = false;

            public UnityEvent<string> onPermissionGranted;

            public UnityEvent<string> onPermissionDenied;
        }

        void Awake()
        {
            if (m_ProcessPermissionsOnStart)
                ProcessPermissions();
        }

        void OnValidate()
        {
            // Ensure that the permission groups are not empty and that they have at least one permission defined.
            for (int i = 0; i < m_PermissionGroups.Count; i++)
            {
                m_PermissionGroups[i].name = m_PermissionGroups[i].platformType.ToString();
            }

        }

        /// <summary>
        /// Process the permissions defined in the <see cref="m_PermissionGroups"/> list.
        /// </summary>
        public void ProcessPermissions()
        {
            m_Persmissions = new List<PermissionRequest>();

#if UNITY_ANDROID

            // Grab the current platform permission group based on the current platform in use.
            m_CurrentPlatformPermissionGroup = m_PermissionGroups.Find(g => g.platformType == XRPlatformUnderstanding.CurrentPlatform || g.platformType == XRPlatformType.All);
            if (m_CurrentPlatformPermissionGroup == null)
            {
                Debug.LogWarning($"No permission group found for platform {XRPlatformUnderstanding.CurrentPlatform}. Disabling {nameof(PermissionsManager)} component.", this);
                enabled = false;
                return;
            }

            // If we have permissions for all platforms, add them to the current platform's permissions.
            if(m_PermissionGroups.Exists(g => g.platformType == XRPlatformType.All))
            {
                m_CurrentPlatformPermissionGroup.permissions.AddRange(m_PermissionGroups.Find(g => g.platformType == XRPlatformType.All).permissions);
            }

            // Loop through the current platform's permissions and add them to the
            // list of permissions to request if they are enabled and not already requested.
            for(int i = 0; i < m_CurrentPlatformPermissionGroup.permissions.Count; i++)
            {
                if (!m_CurrentPlatformPermissionGroup.permissions[i].enabled)
                    continue;

                // If permission is not granted and not requested, add it to the list of permissions to request
                if (!Permission.HasUserAuthorizedPermission(m_CurrentPlatformPermissionGroup.permissions[i].permissionId) && !m_CurrentPlatformPermissionGroup.permissions[i].requested)
                {
                    m_Persmissions.Add(m_CurrentPlatformPermissionGroup.permissions[i]);
                    m_CurrentPlatformPermissionGroup.permissions[i].requested = true;
                }
                else
                {
                    Debug.Log($"User has permission for: {m_CurrentPlatformPermissionGroup.permissions[i].permissionId}", this);
                }
            }

            // Process permissions that were not already granted
            if (m_Persmissions.Count > 0)
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                string[] permissions = new string[m_Persmissions.Count];
                for (int i = 0; i < m_Persmissions.Count; i++)
                {
                    permissions[i] = m_Persmissions[i].permissionId;
                }
                Permission.RequestUserPermissions(permissions, callbacks);
            }
#endif // UNITY_ANDROID
        }

        void OnPermissionGranted(string permissionStr)
        {
            var permission = m_Persmissions.Find(p => p.permissionId == permissionStr);
            if (permission == null)
            {
                Debug.LogWarning($"Permission granted callback received for an unexpected permission request, permission ID {permissionStr}", this);
                return;
            }

            // Enable permission
            permission.granted = true;
            permission.responseReceived = true;

            Debug.Log($"User granted permission for: {permissionStr}", this);
            permission.onPermissionGranted.Invoke(permissionStr);
        }

        void OnPermissionDenied(string permissionStr)
        {
            // Find the permission with LINQ
            var permission = m_Persmissions.Find(p => p.permissionId == permissionStr);
            if (permission == null)
            {
                Debug.LogWarning($"Permission denied callback received for an unexpected permission request, permission ID {permissionStr}", this);
                return;
            }

            // Disable permission
            permission.granted = false;
            permission.responseReceived = true;

            Debug.LogWarning($"User denied permission for: {permissionStr}", this);
            permission.onPermissionDenied.Invoke(permissionStr);
        }
    }
}
