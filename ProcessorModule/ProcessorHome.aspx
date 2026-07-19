<%@ Page Title="Processor Home" Language="C#" MasterPageFile="~/ProcessorModule/ProcessorMaster.master" AutoEventWireup="true" CodeBehind="ProcessorHome.aspx.cs" Inherits="CASApp1.ProcessorModule.ProcessorHome" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f7f9fc;
            margin: 0;
            padding: 0;
        }

        .dashboard-wrapper {
            max-width: 1200px;
            margin: 40px auto;
            padding: 20px;
        }

        .overall-summary {
            text-align: center;
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 30px;
            color: #222;
        }

        .two-column-layout {
            display: flex;
            flex-direction: row;
            gap: 40px;
            justify-content: center;
            flex-wrap: wrap;
        }

        .chart-card {
            background: #fff;
            padding: 30px 25px;
            border-radius: 15px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 520px;
            text-align: center;
        }

            .chart-card h4 {
                font-size: 20px;
                margin-bottom: 20px;
                color: #333;
            }

        .chart-panel {
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        canvas {
            width: 300px !important;
            height: 300px !important;
            margin-bottom: 20px;
        }

        .stats-panel {
            text-align: center;
            font-size: 15px;
            color: #444;
        }

            .stats-panel p {
                margin: 6px 0;
            }

            .stats-panel strong {
                color: #222;
            }

        @media screen and (max-width: 768px) {
            .two-column-layout {
                flex-direction: column;
                align-items: center;
            }

            canvas {
                width: 260px !important;
                height: 260px !important;
            }
        }
    </style>

    <div class="dashboard-wrapper">
        <div class="overall-summary">
            👥 Registered Users: <span id="lblTotalStudents">0</span>
        </div>

        <div class="two-column-layout">

            <!-- Statistics Status -->
            <div class="chart-card">
                <h4>📊 Statistics Computation Validation Status</h4>
                <div class="chart-panel">
                    <canvas id="statcomChart"></canvas>
                    <div class="stats-panel">
                        
                        <p><strong>Pending:</strong> <span id="lblPending">0</span></p>
                        <p><strong>Completed:</strong> <span id="lblCompleted">0</span></p>
                    </div>
                </div>
            </div>

            <!-- Similarity AI Status -->
            <div class="chart-card">
                <h4>📊 Similarity & AI Writing Status</h4>
                <div class="chart-panel">
                    <canvas id="simChart"></canvas>
                    <div class="stats-panel">                    
                        <p><strong>Pending:</strong> <span id="lblSimPending">0</span></p>
                        <p><strong>Completed:</strong> <span id="lblSimCompleted">0</span></p>
                    </div>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hfTotalStudents" runat="server" />
        <asp:HiddenField ID="hfCompleted" runat="server" />
        <asp:HiddenField ID="hfPending" runat="server" />
        <asp:HiddenField ID="hfSimSubmitted" runat="server" />
        <asp:HiddenField ID="hfSimCompleted" runat="server" />
        <asp:HiddenField ID="hfSimPending" runat="server" />

    </div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script>
        window.onload = function () {
            const total = parseInt(document.getElementById('<%= hfTotalStudents.ClientID %>').value);
            const completed = parseInt(document.getElementById('<%= hfCompleted.ClientID %>').value);
            const pending = parseInt(document.getElementById('<%= hfPending.ClientID %>').value);
            const notSubmitted = total - (completed + pending);

            // Statistics Chart
            const ctx = document.getElementById('statcomChart').getContext('2d');
            new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: ["Completed", "Pending"],
                    datasets: [{
                        data: [completed, pending],
                        backgroundColor: ["#0b1c60", "#0d6bfa"]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const totalSum = context.dataset.data.reduce((a, b) => a + b, 0);
                                    const value = context.raw;
                                    const percentage = ((value / totalSum) * 100).toFixed(1);
                                    return `${context.label}: ${value} (${percentage}%)`;
                                }

                            }
                        }
                    }
                }
            });

            document.getElementById("lblTotalStudents").innerText = total;
            document.getElementById("lblCompleted").innerText = completed;
            document.getElementById("lblPending").innerText = pending;

            // Similarity Chart
            const simSubmitted = parseInt(document.getElementById('<%= hfSimSubmitted.ClientID %>').value);
            const simCompleted = parseInt(document.getElementById('<%= hfSimCompleted.ClientID %>').value);
            const simPending = parseInt(document.getElementById('<%= hfSimPending.ClientID %>').value);


            const simCtx = document.getElementById('simChart').getContext('2d');
            new Chart(simCtx, {
                type: 'pie',
                data: {
                    labels: ["Completed", "Pending"],
                    datasets: [{
                        data: [simCompleted, simPending],
                        backgroundColor: ["#0b1c60", "#0d6bfa"]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    const value = context.raw;
                                    const percentage = ((value / total) * 100).toFixed(1);
                                    return `${context.label}: ${value} (${percentage}%)`;
                                }
                            }
                        }
                    }
                }
            });

            document.getElementById("lblSimPending").innerText = simPending;
            document.getElementById("lblSimCompleted").innerText = simCompleted;
        };

    </script>
</asp:Content>