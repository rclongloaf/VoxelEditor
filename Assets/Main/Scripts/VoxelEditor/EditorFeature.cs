using Main.Scripts.VoxelEditor.ActionDelegates;
using Main.Scripts.VoxelEditor.ActionDelegates.Input;
using Main.Scripts.VoxelEditor.Events;
using Main.Scripts.VoxelEditor.Repository;
using Main.Scripts.VoxelEditor.State;
using Main.Scripts.VoxelEditor.View;

namespace Main.Scripts.VoxelEditor
{
    public class EditorFeature
    {
        internal EditorState state = new EditorState.WaitingForProject(false);
        private EditorView view;

        private EditorReducer reducer;

        private LoadVoxActionDelegate loadVoxActionDelegate;
        private LoadTextureActionDelegate loadTextureActionDelegate;
        private SaveVoxActionDelegate saveVoxActionDelegate;
        private ImportActionDelegate importActionDelegate;
        private ExportActionDelegate exportActionDelegate;
        private EditModeActionDelegate editModeActionDelegate;
        private BrushActionDelegate brushActionDelegate;
        private InputActionDelegate inputActionDelegate;
        private SpriteSettingsActionDelegate spriteSettingsActionDelegate;

        public EditorFeature(EditorView view, EditorEventsConsumer eventsConsumer)
        {
            this.view = view;
            
            reducer = new EditorReducer(this);
            var repository = new EditorRepository();
            loadVoxActionDelegate = new LoadVoxActionDelegate(this, reducer, repository, eventsConsumer);
            loadTextureActionDelegate = new LoadTextureActionDelegate(this, reducer, repository, eventsConsumer);
            saveVoxActionDelegate = new SaveVoxActionDelegate(this, reducer, repository, eventsConsumer);
            importActionDelegate = new ImportActionDelegate(this, reducer, repository, eventsConsumer);
            exportActionDelegate = new ExportActionDelegate(this, reducer, eventsConsumer);
            editModeActionDelegate = new EditModeActionDelegate(this, reducer, repository);
            brushActionDelegate = new BrushActionDelegate(this, reducer);
            inputActionDelegate = new InputActionDelegate(this, reducer);
            spriteSettingsActionDelegate = new SpriteSettingsActionDelegate(this, reducer);
        }

        public void ApplyAction(EditorAction action)
        {
            switch (action)
            {
                case EditorAction.LoadVox loadVoxAction:
                    loadVoxActionDelegate.ApplyAction(state, loadVoxAction);
                    break;
                case EditorAction.LoadTexture loadTextureAction:
                    loadTextureActionDelegate.ApplyAction(state, loadTextureAction);
                    break;
                case EditorAction.SaveVox saveVoxAction:
                    saveVoxActionDelegate.ApplyAction(state, saveVoxAction);
                    break;
                case EditorAction.SpriteSettings spriteSettings:
                    spriteSettingsActionDelegate.ApplyAction(state, spriteSettings);
                    break;
                case EditorAction.Import importAction:
                    importActionDelegate.ApplyAction(state, importAction);
                    break;
                case EditorAction.Export exportAction:
                    exportActionDelegate.ApplyAction(state, exportAction);
                    break;
                case EditorAction.EditMode editModeAction:
                    editModeActionDelegate.ApplyAction(state, editModeAction);
                    break;
                case EditorAction.Brush brushAction:
                    brushActionDelegate.ApplyAction(state, brushAction);
                    break;
                case EditorAction.Input inputAction:
                    inputActionDelegate.ApplyAction(state, inputAction);
                    break;
            }
        }

        internal void UpdateState(EditorState state)
        {
            this.state = state;
            
            view.ApplyState(state);
        }
    }
}