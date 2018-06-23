Public Class Form1

    Dim SERVER_GATEWAY As String = "http://localhost/12420/index.php"
    Dim SERVER_KEY As String = "MOONSTONE_0193341859"
    Dim MACHINE_ID As String = "MACHINEMACHINEMACHIN"
    Dim TASK_MANAGER As String = "ENABLED_DEFAULT"

    Dim SELECTED_COMMAND As String = ""
    Dim SERVER_HANDSHAKE_STATUS As String = ""
    Dim SERVER_CONNECTED As String = ""
    Dim COMMAND_UPLOAD As String = ""


    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            Dim httpFetch As New Net.WebClient()
            e.Result = httpFetch.DownloadString(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=backcontrol" & "&type=handshake")
            SERVER_CONNECTED = True

            If SELECTED_COMMAND <> "" And SERVER_HANDSHAKE_STATUS = "OK" Then
                COMMAND_UPLOAD = httpFetch.UploadString(SERVER_GATEWAY & "?key=" & SERVER_KEY & "&machine=" & MACHINE_ID & "&action=backcontrol" & "&type=commandupload", SELECTED_COMMAND)
                SELECTED_COMMAND = ""
            End If
        Catch ex As Exception
            SERVER_CONNECTED = False
        End Try
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If SERVER_CONNECTED = True Then
            TextBox1.BackColor = Color.Green
            TextBox1.Text = "Connected: " & SERVER_GATEWAY
            Dim xmlData As New Xml.XmlDocument()
            Try
                xmlData.LoadXml(e.Result)
                SERVER_HANDSHAKE_STATUS = xmlData.SelectSingleNode("main/request/status").InnerText
                If SERVER_HANDSHAKE_STATUS = "OK" Then
                    If xmlData.SelectSingleNode("main/property").InnerText = "online" Then
                        TextBox2.BackColor = Color.Green
                        TextBox2.Text = "Connected: " & MACHINE_ID
                    ElseIf xmlData.SelectSingleNode("main/property").InnerText = "offline" Then
                        TextBox2.BackColor = Color.OrangeRed
                        TextBox2.Text = "Offline: " & MACHINE_ID
                    End If
                    TextBox3.Text = xmlData.SelectSingleNode("main/data").InnerText
                ElseIf SERVER_HANDSHAKE_STATUS = "ERROR" Then
                    TextBox2.BackColor = Color.OrangeRed
                    TextBox2.Text = xmlData.SelectSingleNode("main/request/details").InnerText
                End If
            Catch ex As Exception
                TextBox1.BackColor = Color.Red
                TextBox1.Text = "Invalid Server Response..."
                TextBox2.BackColor = Color.Red
                TextBox2.Text = "Invalid Server Response..."
                SERVER_HANDSHAKE_STATUS = ""
                SELECTED_COMMAND = ""
            End Try
        Else
            TextBox1.BackColor = Color.Red
            TextBox1.Text = "Connection Error..."
        End If
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Select Case ComboBox1.SelectedIndex
            Case 0 'Disable Task Manager
                SELECTED_COMMAND = "6288493926"
            Case 1 'Enable Task Manager
                SELECTED_COMMAND = "8327160372"
            Case 2 'Terminalte Windows Controller
                SELECTED_COMMAND = "1204829430"
        End Select

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SELECTED_COMMAND = "CLEAR"
    End Sub
End Class
