﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Tcp_Lib;

namespace JeuxDuPendu
{
    public partial class ConnectionForm : Form
    {
        private string PlayerName { get; set; }

        public ConnectionForm()
        {
            InitializeComponent();
        }

        private void serverList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            GameForm mainMenu = new GameForm();
            mainMenu.Closed += (s, args) => this.Close();
            mainMenu.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server server = new Server(PlayerName);
            Task startTask = server.Start();
            this.Hide();
            Multiplayer multiplayer = new Multiplayer(server);
            multiplayer.Closed += (s, args) => this.Close();
            multiplayer.Show();
        }

        private void serverDataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            FillServerDataGrid();
        }

        private Task FillServerDataGrid()
        {
            List<Server> registeredServers = new List<Server>();

            try
            {
                using (GameLib.rsc.ProgramDbContext context = new GameLib.rsc.ProgramDbContext())
                {
                    context.Players.Include(collection => collection.Severs);
                    var servers = context.Players.Select(x => x.Severs).FirstOrDefaultAsync().Result;
                    foreach (var server in servers)
                    {
                        registeredServers.Add(server);
                    }
                }

                serverDataGrid.DataSource = PingServers(registeredServers).Result;

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromException(e);
            }
        }

        private async Task<List<ServerListView>> PingServers(List<Server> servers)
        {
            List<ServerListView> result = new List<ServerListView>();

            try
            {
                foreach (var server in servers)
                {
                    result.Add(new ServerListView()
                    {
                        Id = server.Id,
                        IpAddress = server.CurrentIpAddress.ToString(),
                        IsOnline = false
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        private void connectionTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private async void connectionButton_Click(object sender, EventArgs e)
        {
            var tasks = new List<Task>();
            Client client = new Client(PlayerName);
            await client.ConnectAsync(connectionTextBox.Text);
            if (client.ClientStream.Count > 0)
            {
                this.Hide();
                Multiplayer multiplayer = new Multiplayer(client);
                multiplayer.Closed += (s, args) => this.Close();
                multiplayer.Show();
            }

            await Task.Run(client.LaunchProcess);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PlayerName = textBox1.Text;
        }
    }

    public class ServerListView
    {
        public string IpAddress { get; set; }
        public int Id { get; set; }

        public bool IsOnline { get; set; }
    }
}