const maxViewedBriefings = 5;
const maxCreatedBriefings = 5;

export function addViewedBriefing(briefingId) {
    let viewedBriefingIds = getViewedBriefings();
    
    if(viewedBriefingIds.includes(briefingId)) {
        let index = viewedBriefingIds.indexOf(briefingId);
        viewedBriefingIds.splice(index, 1);
        
        viewedBriefingIds.push(briefingId);
        setViewedBriefings(viewedBriefingIds);
        return;
    }
    
    if(viewedBriefingIds.length >= maxViewedBriefings) {
        viewedBriefingIds.shift();
    }
    
    viewedBriefingIds.push(briefingId);
    setViewedBriefings(viewedBriefingIds);
}

export function addCreatedBriefing(briefingId) {
    let createdBriefingIds = getCreatedBriefings();

    if(createdBriefingIds.length >= maxCreatedBriefings) {
        createdBriefingIds.shift();
    }

    createdBriefingIds.push(briefingId);
    setCreatedBriefings(createdBriefingIds);
}

export function getViewedBriefings() {
    let raw = localStorage.getItem("viewedBriefings");
    if(raw === null || raw === "") return [];

    let split = raw.split(",");
    if(split === null || split.length <= 0) return null;

    return split;
}

export function getCreatedBriefings() {
    let raw = localStorage.getItem("createdBriefings");
    if(raw === null || raw === "") return [];

    let split = raw.split(",");
    if(split === null || split.length <= 0) return null;

    return split;
}

function setViewedBriefings(briefingIds) {
    localStorage.setItem("viewedBriefings", briefingIds.join(","));
}

function setCreatedBriefings(briefingIds) {
    localStorage.setItem("createdBriefings", briefingIds.join(","));
}
