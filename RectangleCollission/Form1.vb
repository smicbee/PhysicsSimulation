Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify

Public Class Form1


    Dim outrender As Bitmap
    Sub update_render(sender As Object, e As UpdateRenderEventArgs)


        Dim render As Bitmap = e.outrender

        If outrender IsNot Nothing Then
            outrender.Dispose()
        End If

        outrender = render.Clone
        Me.BackgroundImage = outrender
        Me.BackgroundImageLayout = ImageLayout.Zoom
        Me.Invalidate()

    End Sub


    Dim t As New Timer





    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Randomize()

        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.DoubleBuffer, True)


        Dim sim As New Simulation(New Drawing.Size(Me.ClientSize.Width, Me.ClientSize.Height))
        AddHandler sim.RenderUpdated, AddressOf update_render
        'AddHandler sim.PhysObjLivetimeExpired, AddressOf livetimeexpired
        sim.Start()


        sim.spawn_random_objs(20)




    End Sub




    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        'e.Graphics.DrawImage(outrender, 0, 0)
    End Sub
End Class
