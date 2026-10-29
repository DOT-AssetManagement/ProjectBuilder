document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('arcDiagram');
    var margin = { top: 10, right: 1, bottom: 6, left: 1 },
        width = 950 - margin.left - margin.right,
        height = (5 * 175) - margin.top - margin.bottom;

    const labelPadding = 240;
    const svg = d3.select('#arcDiagram').append('svg')
        .attr('width', width + labelPadding * 2 + 50)
        .attr('height', height)
        .attr('viewBox', `${-width / 2 - labelPadding} ${-height / 2 - 20} ${width + labelPadding * 2} ${height + 200}`);

    const outerRadius = Math.min(width, height) / 2 - 40;
    const innerRadius = outerRadius - 30;

    const scoreNames = ["Condition and Performance Score", "Equity and Access Score", "Mobility and Economy Score", "Resilience and Environment Score", "Safety Score"];
    const projectNames = arcData.map(d => `${d.Description}`);
    const labels = scoreNames.concat(projectNames); // Scores first

    const matrixSize = projectNames.length + scoreNames.length;
    const matrix = Array.from({ length: matrixSize }, () => new Array(matrixSize).fill(0));

    const projectScores = [];
    arcData.forEach((project) => {
        let obj = {};
        obj.ConditionAndPerformanceScore = project.ConditionAndPerformanceScore;
        obj.EquityAndAccessScore = project.EquityAndAccessScore;
        obj.MobilityAndEconomyScore = project.MobilityAndEconomyScore;
        obj.ResilienceAndEnvironmentScore = project.ResilienceAndEnvironmentScore;
        obj.SafetyScore = project.SafetyScore;
        projectScores.push(obj);
    });

    projectScores.forEach((project, i) => { //18 times
        let j = 0;
        for (const score in project) { // 5 times
            const scoreIndex = j; // Index for score nodes
            const projectIndex = scoreNames.length + i; // Offset by the number of score nodes
            matrix[scoreIndex][projectIndex] = project[score]; // Create the chord
            matrix[projectIndex][scoreIndex] = project[score]; // Mirror for undirected chord
            j++;
        }
    });

    const chord = d3.chord()
        .padAngle(0.02)
        .sortSubgroups(d3.descending)(matrix);

    const arc = d3.arc()
        .innerRadius(innerRadius)
        .outerRadius(outerRadius);

    const ribbon = d3.ribbonArrow()
        .radius(innerRadius - 1)
        .source(d => d.target)  
        .target(d => d.source);  

    const scoreColors = d3.scaleOrdinal(d3.schemeCategory10);
    const projectColor = "#cccccc";  // A grey color for all project nodes

    // Append groups for chords
    const chords = svg.append('g')
        .selectAll('path')
        .data(chord)
        .enter().append('path')
        .attr('class', 'chord')
        .attr('d', ribbon)
        .style('fill', d => scoreColors(d.source.index % scoreNames.length))
        .style('stroke', d => d3.rgb(scoreColors(d.source.index % scoreNames.length)));

    const group = svg.append('g')
        .selectAll('g')
        .data(chord.groups)
        .enter().append('g');

    group.append('path')
        .style('fill', d => d.index < scoreNames.length ? scoreColors(d.index) : projectColor)
        .style('stroke', d => d3.rgb(d.index < scoreNames.length ? scoreColors(d.index) : projectColor).darker())
        .attr('d', arc)
        .on('mouseover', function (event, d) {
            const centroid = arc.centroid(d);
            const x = centroid[0] + width / 2;
            const y = centroid[1] + height / 2;

            chords.classed('highlighted', function (chord) {
                return chord.source.index === d.index || chord.target.index === d.index;
            });

            // Display arc tooltip
            var arcTooltipDiv = document.getElementById('arc-tooltip');
            arcTooltipDiv.classList.add('visible');
            arcTooltipDiv.style.left = `${x}px`;
            arcTooltipDiv.style.top = `${y}px`;
            arcTooltipDiv.innerHTML =
                `<strong>Item: </strong>${labels[d.index]}<br><strong>Total Score: </strong>${Math.round(matrix[d.index].reduce((a, b) => a + b, 0))}`;
        })
        .on('mouseout', function () {
            // Remove highlighting from chords
            d3.selectAll('.chord')
                .classed('highlighted', false);

            // Hide arc tooltip
            document.getElementById('arc-tooltip').classList.remove('visible');
        });

    group.append('text')
        .each(d => { d.angle = (d.startAngle + d.endAngle) / 2; })
        .attr('dy', '.35em')
        .attr('transform', d => {
            let adjustedRotation = '';
            if (d.angle > 0 && d.angle <= Math.PI) {
                adjustedRotation = `rotate(${(Math.PI / 2 - d.angle) * (180 / Math.PI)})`;
            } else {
                adjustedRotation = `rotate(${(Math.PI / 2 - d.angle) * (180 / Math.PI)})`;
            }
            return `rotate(${(d.angle * 180 / Math.PI - 90)}) translate(${outerRadius + 10}) ${adjustedRotation}`;
        })
        .style('text-anchor', d => d.angle > Math.PI ? 'end' : null)
        .text(d => labels[d.index]);

    svg.append('g')
        .attr('fill-opacity', 0.67)
        .selectAll('path')
        .data(chord)
        .enter().append('path')
        .attr('d', ribbon)
        .style('fill', d => scoreColors(d.source.index % scoreNames.length))
        .style('stroke', d => d3.rgb(scoreColors(d.source.index % scoreNames.length)).darker());
});
