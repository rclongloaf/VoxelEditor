using Main.Scripts.Mvp;

namespace Main.Scripts.UI.MainScreen
{
    public interface MainScreenContract : MvpContract
    {
        public interface Presenter : MvpContract.Presenter
        {
            public void OnImportClicked();
            public void OnExportClicked();
            public void OnLoadClicked();
            public void OnSaveClicked();
            public void OnBrushAddClicked();
            public void OnBrushDeleteClicked();
        }

        public interface View : View<Presenter>
        {
            
        }
    }
}