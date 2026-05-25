using System.Text.Json;

namespace RpaScHauStory
{
    /// <summary>FormSettings 폼의 레이아웃(크기·열 너비)을 저장·복원합니다.</summary>
    public class FormSettingsLayout
    {
        public bool UseUserLayout { get; set; } = false;
        public int FormWidth { get; set; } = 900;
        public int FormHeight { get; set; } = 768;
        public Dictionary<string, int> ColumnWidths { get; set; } = new();

        private static readonly string LayoutPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KTJ", "CDP", "formSettings.layout.json");

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public static FormSettingsLayout Load()
        {
            if (!File.Exists(LayoutPath)) return new FormSettingsLayout();
            try
            {
                var json = File.ReadAllText(LayoutPath);
                return JsonSerializer.Deserialize<FormSettingsLayout>(json, JsonOpts) ?? new();
            }
            catch { return new FormSettingsLayout(); }
        }

        public static void Save(FormSettingsLayout layout)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LayoutPath)!);
            File.WriteAllText(LayoutPath, JsonSerializer.Serialize(layout, JsonOpts));
        }
    }
}
