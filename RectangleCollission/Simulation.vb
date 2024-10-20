

Imports System.ComponentModel
Imports System.Drawing.Imaging


Public Class UpdateRenderEventArgs
    Inherits EventArgs
    Public outrender As Bitmap
End Class

Public Class Simulation
    Public Shared objectcounterid As Integer = -1
    Public Shared Function getcounter() As Integer
        Simulation.objectcounterid += 1
        Return objectcounterid
    End Function



    Public Event RenderUpdated(ByVal sender As Object, e As UpdateRenderEventArgs)

    Public Sub New(render_size As Size)
        Me.windowsize = render_size
    End Sub

    Public Sub Start()
        AddHandler renderthread.DoWork, AddressOf render_next_frame
        AddHandler renderthread.ProgressChanged, AddressOf update_render
        renderthread.WorkerReportsProgress = True
        renderthread.RunWorkerAsync()

    End Sub
    Dim collisionobjects As New List(Of RecObj)
    Dim ellapsedticks As Integer = 0
    Dim FPS As Integer = 30
    Dim windowsize As Size

    Dim gravity As PointF = New PointF(0.1, 0.0)
    'Dim gravity As PointF = New PointF(0, 0)
    Dim friction As PointF = New PointF(0.01, 0.01)
    'Dim friction As PointF = New PointF(0.00, 0.000)






    Class RecObj

        Public Sub New(position As RectangleF, movementspeed As PointF, Optional charge As Decimal = 0, Optional mass As Decimal = 1, Optional does_move As Boolean = True, Optional bouncyness As Decimal = 1, Optional img As Image = Nothing, Optional imggif_FPS As Integer = 0)
            Me.position = position
            Dim pencolor As Color
            Dim bordercolor As Color


            Dim maxcharge As Integer = 50

            If charge > 0 Then
                pencolor = Color.Red
                bordercolor = Color.Red
                If Math.Abs(charge) < maxcharge Then
                    pencolor = Color.FromArgb(Math.Abs(charge) / maxcharge * 255, pencolor)
                End If
            ElseIf charge < 0 Then
                pencolor = Color.Blue
                bordercolor = Color.Blue
                If Math.Abs(charge) < maxcharge Then
                    pencolor = Color.FromArgb(Math.Abs(charge) / maxcharge * 255, pencolor)
                End If
            Else
                bordercolor = Color.Black
                pencolor = Nothing
            End If


            Me.fillcolor = pencolor
            Me.border = New Pen(bordercolor, 2)
            Me.img = img
            Me.speed = movementspeed
            Me.id = getcounter()
            Me.charge = charge
            Me.mass = mass
            Me.does_move = does_move
            Me.bouncyness = bouncyness
            Me.gifFPS = imggif_FPS
            Me.activegifframe = 0
        End Sub

        Public gifFPS As Integer
        Public img As Image
        Public activegifframe As Integer
        Public bouncyness As Decimal
        Public does_move As Boolean
        Public fillcolor As Color
        Public id As Integer
        Public position As RectangleF
        Public speed As PointF
        Public border As Pen
        Public charge As Decimal
        Public mass As Decimal
    End Class


    Dim renderthread As New BackgroundWorker


    Public Function Mensch(offsetpt As Point) As CompositeObjekt

        Dim linkerfusspos As New Rectangle(100 + offsetpt.X, 100 + offsetpt.Y, 20, 50)
        Dim rechterfusspos As New Rectangle(200 + offsetpt.X, 100 + offsetpt.Y, 20, 50)

        Dim bauch As New Rectangle(100 + offsetpt.X, 50 + offsetpt.Y, 120, 50)
        Dim kopf As New Rectangle(132 + offsetpt.X, 0 + offsetpt.Y, 50, 50)





        Dim kopfobj As New RecObj(kopf, Nothing, 0, 10000)
        Dim bauchobj As New RecObj(bauch, Nothing, 0, 10000)
        Dim linkerfuss As New RecObj(linkerfusspos, Nothing, 0, 10000)
        Dim rechterfuss As New RecObj(rechterfusspos, Nothing, 0, 10000)


        Dim returnobj As CompositeObjekt = New CompositeObjekt({kopfobj, bauchobj, linkerfuss, rechterfuss})

        Return returnobj
    End Function

    Public Function Blackhole(offsetpt As Point) As CompositeObjekt

        Dim singularitypos As New Rectangle(offsetpt.X, offsetpt.Y, 2, 2)

        Dim singularity As New RecObj(singularitypos, Nothing, 0, 100)


        Dim returnobj As CompositeObjekt = New CompositeObjekt({singularity})

        Return returnobj
    End Function



    Public Function ChristmasGIF(offsetpt As Point, Optional ByVal scale As Decimal = 1) As CompositeObjekt

        Dim gif As Image = Image.FromFile("B:\Google Drive\Desktop\christmas-holiday.gif")


        Dim singularitypos As New Rectangle(offsetpt.X, offsetpt.Y, gif.Width * scale, gif.Height * scale)

        Dim singularity As New RecObj(singularitypos, Nothing, 0, 100, img:=gif)
        singularity.border = Nothing

        Dim returnobj As CompositeObjekt = New CompositeObjekt({singularity})

        Return returnobj
    End Function




    Class CompositeObjekt
        Public Sub New(objs As RecObj())
            singles = objs
        End Sub

        Public singles As RecObj()
    End Class

    Public Function collideswithwall_x(obj As RecObj)

        If obj.position.Left < 0 Then
            obj.position.X = 0
            Return True
        End If

        If obj.position.Right > windowsize.Width Then
            obj.position.X = windowsize.Width - obj.position.Width
            Return True
        End If


        Return False
    End Function

    Public Function collideswithwall_y(obj As RecObj)

        If obj.position.Top < 0 Then
            obj.position.Y = 0
            Return True
        End If

        If obj.position.Bottom > windowsize.Height Then
            obj.position.Y = windowsize.Height - obj.position.Height
            Return True
        End If

        Return False
    End Function


    Public Function collides(obj1 As RecObj, obj2 As RecObj) As Boolean

        If obj1.position.IntersectsWith(obj2.position) Then
            Return True
        End If


        Return False
    End Function

    Public Function collideswithanyotherobj(obj As RecObj) As RecObj()
        Dim collissionlist As New List(Of RecObj)

        For Each obj2 In collisionobjects

            If obj.id = obj2.id Then
                Continue For
            End If


            If collides(obj, obj2) Then
                collissionlist.Add(obj2)
            End If


        Next


        Return collissionlist.ToArray

    End Function

    Sub apply_speed(obj As RecObj)

        If obj.does_move Then
            obj.position.X += obj.speed.X
            obj.position.Y += obj.speed.Y
        End If

    End Sub

    Dim gravity_between_objects As Boolean = False

    Sub apply_gravity(obj As RecObj)
        obj.speed.X += gravity.X
        obj.speed.Y += gravity.Y

        If gravity_between_objects Then


            For Each item In collisionobjects

                If item.id = obj.id Then
                    Continue For
                End If


                Dim r As Decimal = Math.Sqrt((obj.position.X - item.position.X) ^ 2 + (obj.position.Y - item.position.Y) ^ 2)

                Dim chargesquared As Decimal = item.mass * obj.mass / 1000

                Dim F As Decimal = -chargesquared / r ^ 2 / obj.mass


                Dim angle As Decimal = Math.Atan2((obj.position.MidX - item.position.MidX), (obj.position.MidY - item.position.MidY))

                'item.speed.X += Math.Sin(angle) * F
                Dim forcex As Decimal = Math.Sin(angle) * F
                obj.speed.X += forcex

                Dim forcey As Decimal = Math.Cos(angle) * F
                obj.speed.Y += forcey



            Next
        End If

    End Sub

    Sub apply_friction(obj As RecObj)

        If obj.speed.X > 0 Then
            obj.speed.X -= friction.X
        Else
            obj.speed.X += friction.X
        End If

        If obj.speed.Y > 0 Then
            obj.speed.Y -= friction.Y
        Else
            obj.speed.Y += friction.Y
        End If



    End Sub

    Sub apply_charge_rejection(obj As RecObj)

        If obj.charge = 0 Then
            Exit Sub
        End If


        For Each item In collisionobjects

            If item.id = obj.id OrElse item.charge = 0 Then
                Continue For
            End If


            Dim r As Decimal = Math.Sqrt((obj.position.X - item.position.X) ^ 2 + (obj.position.Y - item.position.Y) ^ 2)

            Dim chargesquared As Decimal = item.charge * obj.charge

            Dim F As Decimal = chargesquared / r ^ 2 / obj.mass


            Dim angle As Decimal = Math.Atan2((obj.position.MidX - item.position.MidX), (obj.position.MidY - item.position.MidY))

            'item.speed.X += Math.Sin(angle) * F
            Dim forcex As Decimal = Math.Sin(angle) * F
            obj.speed.X += forcex

            Dim forcey As Decimal = Math.Cos(angle) * F
            obj.speed.Y += forcey



        Next


    End Sub

    Sub render_next_frame(sender As Object, e As DoWorkEventArgs)
        Dim outrender As New Bitmap(windowsize.Width, windowsize.Height)
        Dim g As Graphics = Graphics.FromImage(outrender)
        Dim bck As BackgroundWorker = sender
        Dim fpscalc As New Stopwatch
        Dim effectivefps As New Stopwatch

        While True
            effectivefps.Reset()
            fpscalc.Reset()

            effectivefps.Start()
            fpscalc.Start()

            g.Clear(Color.White)

            Dim colldup = collisionobjects.ToList

            For Each obj In colldup

                apply_friction(obj)
                apply_gravity(obj)
                apply_speed(obj)
                apply_charge_rejection(obj)

                If collideswithwall_x(obj) Then
                    obj.speed.X = -obj.speed.X
                    obj.position.X += obj.speed.X
                    obj.speed.X = obj.speed.X * obj.bouncyness
                End If


                If collideswithwall_y(obj) Then
                    obj.speed.Y = -obj.speed.Y
                    obj.position.Y += obj.speed.Y
                    obj.speed.Y = obj.speed.Y * obj.bouncyness
                End If

                Dim collideswith As RecObj() = collideswithanyotherobj(obj)

                For Each collobj In collideswith
                    ResolveCollision(obj, collobj)
                Next

                Dim roundedrectangle As Rectangle = New Rectangle(Math.Abs(obj.position.X), Math.Abs(obj.position.Y), Math.Abs(obj.position.Width), Math.Abs(obj.position.Height))

                If obj.border IsNot Nothing Then
                    g.DrawRectangle(obj.border, roundedrectangle)
                End If
                If obj.img IsNot Nothing Then
                    Dim framedimensions As Imaging.FrameDimension = New FrameDimension(obj.img.FrameDimensionsList(0))
                    obj.img.SelectActiveFrame(framedimensions, ellapsedticks Mod obj.img.GetFrameCount(framedimensions))
                    g.DrawImage(obj.img, obj.position)
                End If

                g.FillRectangle(New SolidBrush(obj.fillcolor), obj.position)


            Next

            collisionobjects = colldup


            fpscalc.Stop()


            bck.ReportProgress(99, outrender)

            Dim mstowait As Decimal = ((1000 / FPS) - fpscalc.ElapsedMilliseconds)

            If mstowait > 0 Then
                System.Threading.Thread.Sleep(mstowait)
            Else
                g.DrawString("Can not keep up frames", New Font("Arial", 12), New SolidBrush(Color.Red), 0, 0)
            End If

            ellapsedticks += 1

        End While

        g.Dispose()

    End Sub

    Public Sub ResolveCollision(obj1 As RecObj, obj2 As RecObj)
        ' Calculate overlap in both axes
        Dim overlapX As Single = Math.Min(obj1.position.X + obj1.position.Width, obj2.position.X + obj2.position.Width) - Math.Max(obj1.position.X, obj2.position.X)
        Dim overlapY As Single = Math.Min(obj1.position.Y + obj1.position.Height, obj2.position.Y + obj2.position.Height) - Math.Max(obj1.position.Y, obj2.position.Y)

        ' Adjust position and velocity based on the smallest overlap
        If Math.Abs(overlapX) < Math.Abs(overlapY) Then
            ' Horizontal collision: reverse X velocities
            If obj1.position.X < obj2.position.X Then
                obj1.position.X -= overlapX / 2
                obj2.position.X += overlapX / 2
            Else
                obj1.position.X += overlapX / 2
                obj2.position.X -= overlapX / 2
            End If

            ' Apply conservation of momentum for horizontal velocities
            Dim newVelX1 As Single = ((obj1.mass - obj2.mass) * obj1.speed.X + 2 * obj2.mass * obj2.speed.X) / (obj1.mass + obj2.mass)
            Dim newVelX2 As Single = ((obj2.mass - obj1.mass) * obj2.speed.X + 2 * obj1.mass * obj1.speed.X) / (obj1.mass + obj2.mass)
            obj1.speed.X = newVelX1 * obj1.bouncyness
            obj2.speed.X = newVelX2 * obj2.bouncyness

        Else
            ' Vertical collision: reverse Y velocities
            If obj1.position.Y < obj2.position.Y Then
                obj1.position.Y -= overlapY / 2
                obj2.position.Y += overlapY / 2
            Else
                obj1.position.Y += overlapY / 2
                obj2.position.Y -= overlapY / 2
            End If

            ' Apply conservation of momentum for horizontal velocities
            Dim newVelY1 As Single = ((obj1.mass - obj2.mass) * obj1.speed.Y + 2 * obj2.mass * obj2.speed.Y) / (obj1.mass + obj2.mass)
            Dim newVelY2 As Single = ((obj2.mass - obj1.mass) * obj2.speed.Y + 2 * obj1.mass * obj1.speed.Y) / (obj1.mass + obj2.mass)
            obj1.speed.Y = newVelY1 * obj1.bouncyness
            obj2.speed.Y = newVelY2 * obj2.bouncyness
        End If
    End Sub


    Dim outrender As Bitmap



    Sub update_render(sender As Object, e As ProgressChangedEventArgs)

        Dim render As Bitmap = e.UserState

        If outrender IsNot Nothing Then
            outrender.Dispose()
        End If

        outrender = render.Clone

        Dim ev As New UpdateRenderEventArgs
        ev.outrender = outrender

        RaiseEvent RenderUpdated(Me, ev)
    End Sub


    Dim t As New Timer

    Sub add_new_obj()
        Dim pos As New Rectangle(Rnd() * windowsize.Width, Rnd() * windowsize.Height, 20, 20)
        Dim newobj As New RecObj(pos, New Drawing.SizeF(Rnd() * 5, Rnd() * 5))

        If collideswithanyotherobj(newobj).Count = 0 AndAlso Not collideswithwall_x(newobj) AndAlso Not collideswithwall_y(newobj) Then
            collisionobjects.Add(newobj)
        End If


    End Sub




    Sub dreikörperproblem()


        Dim pos1 As New RectangleF(400, 250, 20, 20)
        Dim newobj1 As New RecObj(pos1, New Drawing.SizeF(0, 0), 200, 100000)
        collisionobjects.Add(newobj1)


        Dim pos2 As New RectangleF(400, 150, 20, 20)
        Dim newobj2 As New RecObj(pos2, New Drawing.SizeF(4, -1), -100, 10)
        collisionobjects.Add(newobj2)


        Dim pos3 As New RectangleF(400, 350, 20, 20)
        Dim newobj3 As New RecObj(pos3, New Drawing.SizeF(-4, 1), -100, 10)
        collisionobjects.Add(newobj3)

    End Sub


    Sub spawn_random_objs_christmas(num As Integer)

        Dim maxcharge As Integer = 0
        For i = 0 To num
retry:
            Dim pos As New Rectangle(Rnd() * windowsize.Width, Rnd() * windowsize.Height, Rnd() * 200 + 20, Rnd() * 200 + 20)

            Dim c As CompositeObjekt = ChristmasGIF(New Point(Rnd() * windowsize.Width, Rnd() * windowsize.Height), 0.2)

            If collideswithanyotherobj(c.singles(0)).Count = 0 AndAlso collideswithwall_x(c.singles(0)) = False AndAlso collideswithwall_y(c.singles(0)) = False Then
                'c.Load()

            Else
                GoTo retry
            End If

        Next

    End Sub


    Sub spawn_random_objs(num As Integer)

        Dim maxcharge As Integer = 0
        For i = 0 To num
retry:
            Dim pos As New Rectangle(Rnd() * windowsize.Width, Rnd() * windowsize.Height, Rnd() * 200 + 20, Rnd() * 200 + 20)

            Dim newobj As New RecObj(pos, Nothing, 0, 100, True, 0.6)

            If collideswithanyotherobj(newobj).Count = 0 AndAlso collideswithwall_x(newobj) = False AndAlso collideswithwall_y(newobj) = False Then
                collisionobjects.Add(newobj)
            Else
                GoTo retry
            End If

        Next
    End Sub



End Class
