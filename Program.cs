using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;

public class Program
{
    // Importar funciones de la API de Windows
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    public static void Main(string[] args)
    {
        // Nombre del proceso de Spotify
        string processName = "Spotify";

        // Adjuntarse a la instancia existente de Spotify
        var app = AttachToApplication(processName);
        if (app == null)
        {
            Console.WriteLine("Spotify no está abierto.");
            return;
        }

        // Traer la ventana de Spotify al frente
        FocusSpotifyWindow();

        using (var automation = new UIA3Automation())
        {
            AutomationElement mainPanel = null;

            // Intentar obtener el panel de Spotify
            
            var allElements = automation.GetDesktop().FindAllChildren(cf => cf.ByControlType(ControlType.Pane));
            Console.WriteLine($"Se encontraron {allElements.Length} paneles.");

            // Buscar el panel de Spotify
            foreach (var element in allElements)
            {
                Console.WriteLine($"Nombre del elemento: {element.Name}");
                if (element.Name.Contains("Spotify Premium"))
                {
                    mainPanel = element;
                    break;
                }
            }


            if (mainPanel == null)
            {
                Console.WriteLine("No se pudo encontrar el panel principal de Spotify.");
                return;
            }

            // Esperar a que el panel principal esté disponible
            mainPanel.WaitUntilClickable();

            var playButton = mainPanel
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Document))
                .FindAllDescendants(panes => panes.ByControlType(ControlType.Group));

            var customfilter = mainPanel.FindFirst(TreeScope.Descendants,
                            new AndCondition(
                                mainPanel.ConditionFactory.ByControlType(ControlType.Button),
                                mainPanel.ConditionFactory.ByName("Reproducir")
                            )
                        );



            //.Skip(1)
            //.Take(1)
            //.First()  
            //.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName("Lista 3 Playlist • AlejandroHurtado")));

            // Navegar en el árbol de elementos para encontrar el botón "Reproducir"
            //var playButton = mainPanel
            //    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Document))?
            //    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Group).And(cf.ByName("Está escuchando:")))?

            if (playButton == null)
            {
                Console.WriteLine("No se encontró el botón 'Reproducir'.");
                return;
            }

            // Hacer clic en el botón "Reproducir"
            customfilter.AsButton().Invoke();

            // Esperar un momento para asegurarse de que la canción comienza a reproducirse
            Thread.Sleep(2000);
        }
    }

    private static void FocusSpotifyWindow()
    {
        IntPtr desktopHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Program", null);
        IntPtr spotifyHandle = FindWindowEx(desktopHandle, IntPtr.Zero, null, "Spotify Premium");

        if (spotifyHandle != IntPtr.Zero)
        {
            SetForegroundWindow(spotifyHandle);
            Console.WriteLine("Spotify se ha traído al frente.");
        }
        else
        {
            Console.WriteLine("No se pudo encontrar la ventana de Spotify.");
        }
    }

    private static Application AttachToApplication(string processName)
    {
        // Buscar el primer proceso con el nombre especificado
        var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
        if (process != null)
        {
            return Application.Attach(process.Id);
        }
        return null;
    }
}
