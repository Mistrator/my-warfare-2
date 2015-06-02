using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

public class AIUpdater
{
    /// <summary>
    /// Miten usein päivitetään vihollisten reitinhaku.
    /// </summary>
    public Timer TekoalynPaivitysAjastin { get; set; }

    /// <summary>
    /// Jono, jolla tasapainotetaan reitinhaun lagipiikkejä
    /// </summary>
    public Queue<Vihollinen> ReitinPaivitysJono { get; set; }

    /// <summary>
    /// Miten usein lasketaan reitit jonossa oleville vihollisille.
    /// </summary>
    private Timer ReitinLaskentaAjastin { get; set; }

    private List<Vihollinen> viholliset;

    public void InitializeAI(List<Vihollinen> viholliset)
    {
        this.TekoalynPaivitysAjastin = new Timer();
        this.TekoalynPaivitysAjastin.Interval = Vakiot.AI_REFRESH_RATE;
        this.TekoalynPaivitysAjastin.Timeout += AITick;
        this.TekoalynPaivitysAjastin.Start();

        ReitinPaivitysJono = new Queue<Vihollinen>();

        this.ReitinLaskentaAjastin = new Timer();
        this.ReitinLaskentaAjastin.Interval = Vakiot.DIJKSTRA_PATHFINDING_REFRESH_RATE;
        this.ReitinLaskentaAjastin.Timeout += HandlePathfindingQueue;
        this.ReitinLaskentaAjastin.Start();

        this.viholliset = viholliset;
    }

    public void Stop()
    {
        this.TekoalynPaivitysAjastin.Stop();
        this.ReitinLaskentaAjastin.Stop();
    }

    private void AITick()
    {
        for (int i = 0; i < viholliset.Count; i++)
        {
            viholliset[i].PaivitaTekoaly();
        }
    }

    private void HandlePathfindingQueue()
    {
        if (ReitinPaivitysJono == null || ReitinPaivitysJono.Count == 0) return;

        int updates = Math.Min(ReitinPaivitysJono.Count, Vakiot.AI_PATHFINDING_UPDATES_PER_TICK);

        for (int i = 0; i < updates; i++)
        {
            Vihollinen v = ReitinPaivitysJono.Dequeue();
            if (v.NextTarget != null)
                v.FindAndUseRoute();
        }
    }

    public void EnqueueForPathfindingUpdate(Vihollinen v)
    {
        ReitinPaivitysJono.Enqueue(v);
    }

    /// <summary>
    /// Päivitetään vihollisten reitit tarpeen mukaan lisättyjen seinien varalta.
    /// </summary>
    public void UpdateRoutesForAddedWalls()
    {
        if (MW2_My_Warfare_2_.Peli.KentanOsat == null) return;

        for (int i = 0; i < viholliset.Count; i++)
        {
            if (!viholliset[i].HasRoute) continue;

            for (int j = 0; j < viholliset[i].ReittiAivot.Path.Count; j++)
            {
                if (MW2_My_Warfare_2_.Peli.KentanOsat.GetNodeFromIndex(MW2_My_Warfare_2_.Peli.KentanOsat.GetCorrespondingNode(viholliset[i].ReittiAivot.Path[j])).IsFull)
                {
                    viholliset[i].HasRoute = false;
                    viholliset[i].PaivitaTekoaly();
                }
            }
        }
    }
}