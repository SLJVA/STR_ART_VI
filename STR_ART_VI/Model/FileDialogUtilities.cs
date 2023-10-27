using Microsoft.Win32;

namespace STR_ART_VI.Model
{
    public static class FileDialogUtilities
    {
        public static string? OpenImageFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() is true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public static string? SaveImageFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() is true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }
    }
}
