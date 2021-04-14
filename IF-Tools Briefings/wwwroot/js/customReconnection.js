let reconnectModal = document.querySelector(".reconnect-modal");
let stateReconnecting = document.querySelector(".state-reconnecting");
let stateLost = document.querySelector(".state-lost");
let stateRefused = document.querySelector(".state-refused");

// custom onConnectionDown handler
async function connectionDown(options) {
    reconnectModal.classList.remove("hide");
    hideAllStates();
    stateReconnecting.classList.remove("hide");

    for (let i = 0; i < options.maxRetries; i++) {
        await this.delay(options.retryIntervalMilliseconds);

        if (this.isDisposed) {
            break;
        }

        try {
            const result = await window.Blazor.reconnect();

            if (!result) {
                console.error("(reconnect attempt #" + i + ") Server refused.");
            } else {
                hideAllStates();
                reconnectModal.classList.add("hide");

                return;
            }
        } catch { }
    }

    hideAllStates();
    stateLost.classList.remove("hide");
}

function delay(durationMilliseconds) {
    return new Promise(resolve => setTimeout(resolve, durationMilliseconds));
}

function hideAllStates() {
    stateLost.classList.add("hide");
    stateReconnecting.classList.add("hide");
    stateRefused.classList.add("hide");
}

// custom onConnectionUp handler
function connectionUp(e) {
    hideAllStates();
    reconnectModal.classList.add("hide");
}

window.Blazor.start({
    reconnectionOptions: {
        maxRetries: 30,
        retryIntervalMilliseconds: 500,
    },
    reconnectionHandler: {
        onConnectionDown: e => connectionDown(e),
        onConnectionUp: e => connectionUp(e)
    }
});
