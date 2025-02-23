﻿let axiosBarCharData = null;

// get data from backend
async function getBarChartDataFromDb() {
    try {
        axiosBarCharData = await axios.get("http://localhost:5183/Home/GetBarChartData",
            {
                Headers: {
                    "content-type": "application/json;"
                },
            }
        );
        if (axiosBarCharData.status == 200) {
            console.log(axiosBarCharData.data);
        };
    } catch (err) {
        console.error(`Error fetching data: ${err.message}`);
    }
}

getBarChartDataFromDb();

let myplot = document.querySelector("#myplot");

// Declare the chart dimensions and margins.
const width = 900;
const height = 500;
const marginTop = 20;
const marginRight = 20;
const marginBottom = 30;
const marginLeft = 40;



// Declare the x (horizontal position) scale.
const x = d3.scaleUtc()
    .domain([new Date("2023-01-01"), new Date("2024-01-01")])
    .range([marginLeft, width - marginRight]);

// Declare the y (vertical position) scale.
const y = d3.scaleLinear()
    .domain([0, 100])
    .range([height - marginBottom, marginTop]);

// Create the SVG container.
const svg = d3.create("svg")
    .attr("width", width)
    .attr("height", height);

// Add the x-axis.
svg.append("g")
    .attr("transform", `translate(0,${height - marginBottom})`)
    .call(d3.axisBottom(x));

// Add the y-axis.
svg.append("g")
    .attr("transform", `translate(${marginLeft},0)`)
    .call(d3.axisLeft(y));

// Append the SVG element.
myplot.append(svg.node());

