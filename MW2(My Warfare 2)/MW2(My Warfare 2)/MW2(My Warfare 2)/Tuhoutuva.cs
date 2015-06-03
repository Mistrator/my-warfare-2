using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;


/// <summary>
/// Annetaan seinille kestolaskuri.
/// </summary>
public class Tuhoutuva : PhysicsObject
{
    private DoubleMeter kesto;
    public DoubleMeter Kesto
    {
        get { return kesto; }
        set { kesto = value; }
    }

    public bool Shatters { get; set; }

    public Tuhoutuva(double width, double height, double kesto)
        : base(width, height, Shape.Rectangle)
    {
        this.kesto = new DoubleMeter(kesto);
        this.kesto.MinValue = 0;
        IsUpdated = true;
        Shatters = true;
        this.CreateShard += Shard;
        Ignited += delegate(PhysicsObject p) {
            Flame f = Partikkelit.CreateFlames(this.Position + RandomGen.NextVector(-this.Width / 4, this.Width / 4), 100);
            Extinguished += delegate(PhysicsObject p2) {
                this.Destroy();
                f.FadeOut(2);
                Timer.SingleShot(2, delegate { f.Destroy(); });
            };
        };
        this.kesto.LowerLimit += Extinquish;
        this.kesto.LowerLimit += HajotaPaloiksi;
    }

    public override void Update(Time time)
    {
        if (!MW2_My_Warfare_2_.Peli.OnkoNaytonAlueella(this.Position)) this.Body.IsCollidable = false;
        else this.Body.IsCollidable = true;
        base.Update(time);
    }

    public void HajotaPaloiksi()
    {
        if (!Shatters) return;
        if (this.IsShattered) return;

        Shatter(Vakiot.SHATTER_SIZE, Vector.Zero);
    }

    public PhysicsObject Shard(double width, double height, Vector position)
    {
        PhysicsObject shard = new PhysicsObject(width, height);
        shard.Position = position;
        MW2_My_Warfare_2_.Peli.Efektit.LisaaSirpale(shard);

        shard.LinearDamping = RandomGen.NextDouble(0.80, 0.90);
        shard.AngularDamping = 0.90;
        shard.CanBurn = false;
        shard.Mass = 1;
        shard.Hit(RandomGen.NextVector(100.0, 500.0));

        return shard;
    }

        /*Vector maxShardSize = new Vector(this.Width, this.Height);
        ObjSlicer(this,
            50, maxShardSize, 5); // 4
        this.Destroy();

    }

    private void ObjSlicer(PhysicsObject obj, int maxShards, Vector maxShardSize, double minShardSize)
    {
        const double KESTON_VAHENEMINEN = 1.5; // 2
        double divider = 1.6;

        maxShardSize.X /= divider;
        maxShardSize.Y /= divider;

        double maxSize = 0;
        if (maxShardSize.X > maxShardSize.Y) maxSize = maxShardSize.X;
        else maxSize = maxShardSize.Y;


        // Keskimmäisen suorakulmion paikka
        Vector pos = new Vector(obj.X, obj.Y);

        Tuhoutuva parent = CreateBox(pos.X, pos.Y, maxShardSize.X, maxShardSize.Y, "normal", minShardSize, maxSize, obj.Angle, this.kesto / KESTON_VAHENEMINEN); // keski(parent)
        if (parent == null)
        {
            return;
        }
        else
        {
            parent.Velocity = obj.Velocity;
            parent.AngularVelocity = obj.AngularVelocity;
        }

        double width = parent.Width;
        double height = obj.Top - parent.Top;

        Tuhoutuva box = new Tuhoutuva(1, 1, 1);

        // parent on keskimmäinen suorakulmio
        pos = parent.Position + Vector.FromLengthAndAngle(parent.Height / 2 + height / 2, parent.Angle + Angle.RightAngle);
        PhysicsObject topbox = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, parent.Angle, this.kesto); //ylä kesk

        if (topbox != null)
        {
            topbox.Velocity = obj.Velocity;

            width = parent.Left - obj.Left;

            pos = topbox.Position - Vector.FromLengthAndAngle(topbox.Width / 2 + width / 2, topbox.Angle);// - (obj.Angle));
            box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, topbox.Angle, this.kesto / KESTON_VAHENEMINEN); //ylä vas

            if (box != null)
            {
                box.Velocity = parent.Velocity;
                box.AngularVelocity = parent.AngularVelocity;
            }

            width = obj.Right - parent.Right;

            pos = topbox.Position + Vector.FromLengthAndAngle(topbox.Width / 2 + width / 2, topbox.Angle);// - (obj.Angle));
            box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, topbox.Angle, this.kesto / KESTON_VAHENEMINEN); //ylä oik

            if (box != null)
            { box.Velocity = obj.Velocity; box.AngularVelocity = parent.AngularVelocity; }
        }

        width = obj.Right - parent.Right;

        height = parent.Height;

        pos = parent.Position + Vector.FromLengthAndAngle(parent.Width / 2 + width / 2, parent.Angle);
        box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, parent.Angle, this.kesto / KESTON_VAHENEMINEN); //keski oik

        if (box != null)
        { box.Velocity = obj.Velocity; box.AngularVelocity = parent.AngularVelocity; }

        width = parent.Left - obj.Left;

        pos = parent.Position - Vector.FromLengthAndAngle(parent.Width / 2 + width / 2, parent.Angle);
        box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, parent.Angle, this.kesto / KESTON_VAHENEMINEN); //keski vas

        width = parent.Width;
        height = parent.Bottom - obj.Bottom;

        pos = parent.Position + Vector.FromLengthAndAngle(parent.Height / 2 + height / 2, parent.Angle - Angle.RightAngle);
        Tuhoutuva BotBox = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, parent.Angle, this.kesto / KESTON_VAHENEMINEN); //ala kesk

        if (BotBox != null)
        {

            BotBox.Velocity = obj.Velocity;

            width = parent.Left - obj.Left;

            pos = BotBox.Position - Vector.FromLengthAndAngle(BotBox.Width / 2 + width / 2, BotBox.Angle);
            box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, BotBox.Angle, this.kesto / KESTON_VAHENEMINEN); //ala vas

            if (box != null) { box.Velocity = obj.Velocity; box.AngularVelocity = parent.AngularVelocity; }

            width = obj.Right - parent.Right;

            pos = BotBox.Position + Vector.FromLengthAndAngle(BotBox.Width / 2 + width / 2, BotBox.Angle);// - (obj.Angle));
            box = CreateBox(pos.X, pos.Y, width, height, "normal", minShardSize, maxSize, BotBox.Angle, this.kesto / KESTON_VAHENEMINEN); //ala oik

            if (box != null)
            { box.Velocity = obj.Velocity; box.AngularVelocity = parent.AngularVelocity; }
        }
    }

    Tuhoutuva CreateBox(double x, double y, double w, double h, string type, double min, double max, Angle kulma, double kesto)
    {
        if (destructionsInSecond > MAX_DESTRUCTIONS_IN_SECOND) return null;

        if (w < min || w > max)
        {
            return null;
        }
        if (h < min || h > max)
        {
            return null;
        }

        Tuhoutuva box = new Tuhoutuva(w, h, kesto);
        box.X = x;
        box.Y = y;
        box.Angle = kulma;
        box.Image = this.Image;
        box.LinearDamping = 0.85;
        box.AngularDamping = 0.90;
        box.CanBurn = false;

        box.Mass = 1;
        MW2_My_Warfare_2_.Peli.Add(box);

        MW2_My_Warfare_2_.Peli.sirpaleet.Enqueue(box);
        destructionsInSecond++;
        if (MW2_My_Warfare_2_.Peli.sirpaleet.Count > Vakiot.SIRPALEIDEN_MAX_MAARA)
        {
            MuutaTehosteObjektiksi(MW2_My_Warfare_2_.Peli.sirpaleet.Dequeue());
        }

        return box;
    }*/
}