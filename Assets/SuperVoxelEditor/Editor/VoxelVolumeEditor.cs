using SemagGames.SuperVoxelEditor;
using SuperVoxelEditor.Editor.BuildModes;
using SuperVoxelEditor.Editor.BuildTools;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    [CustomEditor(typeof(VoxelVolume))]
    public sealed class VoxelVolumeEditor : UnityEditor.Editor
    {
        public VoxelVolume Volume => (VoxelVolume)target;
        public VoxelVolumeBuildMode ActiveBuildMode => buildModeController.ActiveBuildMode;
        
        public Vector3 VoxelPosition => Raycaster.VoxelPosition;
        
        public VoxelVolumeInspector Inspector { get; private set; }
        public BuildToolController BuildTools { get; private set; }
        public SceneVoxelRaycaster Raycaster { get; private set; }
        
        private VoxelOperations voxelOperations;
        private SceneGuiEventProcessor sceneGuiEventProcessor;
        private BuildModeController buildModeController;
        
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            
            Inspector = new VoxelVolumeInspector();
            BuildTools = new BuildToolController();
            sceneGuiEventProcessor = new SceneGuiEventProcessor(this);
            voxelOperations = new VoxelOperations(this);
            Raycaster = new SceneVoxelRaycaster(this);
            buildModeController = new BuildModeController();
            
            BuildTools.Inspector.BuildToolChanged += OnBuildToolChanged;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            BuildTools.Inspector.BuildToolChanged -= OnBuildToolChanged;
        }

        public override void OnInspectorGUI() => Inspector.DrawInspectorGUI(this, serializedObject);

        public void SwitchBuildMode(BuildMode buildMode) => buildModeController.SwitchBuildMode(buildMode);
        
        public void SetVoxel(Vector3 voxelPosition) => voxelOperations.SetVoxel(voxelPosition);

        public void SetVoxels(Vector3[] voxelPositions) => voxelOperations.SetVoxels(voxelPositions);

        private void OnSceneGUI(SceneView sceneView) => sceneGuiEventProcessor.ProcessSceneGUIEvents(sceneView);
        
        private void OnBuildToolChanged(BuildTool _) => Repaint();
    }
}