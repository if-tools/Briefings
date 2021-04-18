export function createFilePond(initialIds) {
    let initialFiles = [];
    
    JSON.parse(initialIds).forEach(id => {
        if(id.trim() === "") return;
        
        let initialFile = {
            source: id,
            options: {
                type: 'local'
            }
        };
        
        initialFiles.push(initialFile);
    });
    
    FilePond.create(document.querySelector(".filepond"), {
        files: initialFiles
    });
}

export function destroyFilePond() {
    FilePond.destroy(document.querySelector(".filepond"));
}

export function registerEvents() {
    window.onbeforeunload = function() {
        return "This briefing will not be saved. Are you sure?";
    };
}

export function unregisterEvents() {
    window.onbeforeunload = null;
}

export function getFilepondFileIds() {
    let result = [];

    FilePond.find(document.querySelector(".filepond")).getFiles().forEach(entry => result.push(entry.serverId));
    
    return result.filter(i => i != null);
}

export function checkIfFilepondBusy() {
    let status = FilePond.find(document.querySelector(".filepond")).status;
    
    return status === 3;
}
