using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using System.Threading.Tasks;

public class Program
{

    private static bool _try = false;

    // Importar funciones de la API de Windows
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    // Importar funciones de la API de Windows
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_CLOSE = 0x0010;


    public static void Main(string[] args)
    {
        // Traer la ventana de Spotify al frente
        FocusSpotifyWindow();

        using (var automation = new UIA3Automation())
        {
            AutomationElement mainPanel = null;

            // Intentar obtener el panel de Spotify

            var allElements = automation.GetDesktop().FindAllChildren(cf => cf.ByControlType(ControlType.Window));

            // Buscar el panel de Spotify
            foreach (var element in allElements)
            {
                if (element.Name.Contains("Acceso de usuarios"))
                {
                    mainPanel = element;
                }
            }


            if (mainPanel != null)
            {

                // Esperar a que el panel principal esté disponible
                mainPanel.WaitUntilClickable();

                var userInput = mainPanel
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Group))
                    .FindFirstDescendant(panes => panes.ByControlType(ControlType.Pane).And(panes.ByAutomationId("Loginpanel")))
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("textBoxUsuarios")));

                var passInput = mainPanel
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Group))
                    .FindFirstDescendant(panes => panes.ByControlType(ControlType.Pane).And(panes.ByAutomationId("Loginpanel")))
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("textBoxUsuarios")));

                var btnAcceptar = mainPanel
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Group))
                    .FindFirstDescendant(panes => panes.ByControlType(ControlType.Pane).And(panes.ByAutomationId("Loginpanel")))
                    .FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("BtnAceptar")));


                //if (userInput != null)
                //{
                //    userInput.AsTextBox().Enter("ahurtado");
                //}

                //if (passInput != null)
                //{
                //    passInput.AsTextBox().Enter("ahurtado");
                //}

                if (btnAcceptar != null)
                {
                    btnAcceptar.AsButton().Invoke();
                }
            }

            AutomationElement homePanel = null;

            // Buscar el panel de Spotify
            foreach (var element in automation.GetDesktop().FindAllChildren(cf => cf.ByControlType(ControlType.Window)))
            {
                if (element.Name.Contains("Main"))
                {
                    homePanel = element;
                }
            }

            // Esperar a que el panel principal esté disponible
            homePanel.WaitUntilClickable();

            var auditWindow = homePanel
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuBar).And(cf.ByAutomationId("menuStrip")))
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuItem).And(cf.ByName("Audit Activities")))
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuItem).And(cf.ByName("Audit")));


            auditWindow.AsMenuItem().Invoke();

            var invoices = homePanel
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Window).And(cf.ByAutomationId("FacturasGeneradas")));

            invoices.WaitUntilClickable();

            var searchInput = invoices
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Pane).And(cf.ByAutomationId("panel1")))
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit).And(cf.ByAutomationId("textInvoice")));

            var searchBtn = invoices
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Pane).And(cf.ByAutomationId("panel1")))
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("BtnBuscar")));

            var printBtn = invoices
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Pane).And(cf.ByAutomationId("panel2")))
                .FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByAutomationId("AsignaInvoice")));

            if (searchInput != null)
            {
                searchInput.AsTextBox().Enter("C08-D1182650");

                if (searchBtn != null) searchBtn.AsButton().Invoke();

                if (printBtn != null) printBtn.AsButton().Invoke();
            }

            Console.ReadLine();

            // Esperar un momento para asegurarse de que la canción comienza a reproducirse
            Thread.Sleep(2000);
        }
    }

    private static void FocusSpotifyWindow()
    {
        IntPtr desktopHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Program", null);
        IntPtr spotifyHandle = FindWindowEx(desktopHandle, IntPtr.Zero, "WindowsForms10.Window.8.app.0.141b42a_r9_ad1", null);

        if (spotifyHandle != IntPtr.Zero)
        {
            SetForegroundWindow(spotifyHandle);
            Console.WriteLine("App se ha traído al frente.");
        }
        else
        {
            Console.WriteLine("No se pudo encontrar la ventana.");
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
