FilePond.registerPlugin(
    FilePondPluginFileEncode,
    FilePondPluginFileValidateSize,
    FilePondPluginFileValidateType,
    FilePondPluginImageExifOrientation,
    FilePondPluginImagePreview
);

FilePond.setOptions({
    allowReorder: true,
    acceptedFileTypes: ['image/png', 'image/jpeg'],
    maxParallelUploads: 1,
    server: {
        url: "/api/Attachment/",
        process:(fieldName, file, metadata, load, error, progress, abort) => {
            const formData = new FormData();
            formData.append("file", file);

            const request = new XMLHttpRequest();
            request.open('POST', '/api/Attachment/Create');

            request.upload.onprogress = (e) => {
                progress(e.lengthComputable, e.loaded, e.total);
            };

            request.onload = function () {
                if (request.status >= 200 && request.status < 300) {
                    load(request.responseText);
                }
                else {
                    error('Error during upload.');
                }
            };

            request.send(formData);
            return {
                abort: () => {
                    request.abort();
                    abort();
                }
            };
        },
        fetch: null,
        revert: "Revert",
        remove: null,
        load: (source, load, error, progress, abort, headers) => {
            const myRequest = new Request(source);
            fetch(myRequest).then(function(response) {
                response.blob().then(function(myBlob) {
                    load(myBlob)
                });
            });
        }
    }
});
