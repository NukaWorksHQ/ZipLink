window.getBoundingClientRect = (element) => {
    if (!element || typeof element.getBoundingClientRect !== 'function') {
        console.error('Invalid element passed to getBoundingClientRect');
        return null;
    }

    const rect = element.getBoundingClientRect();

    return {
        top: Math.round(rect.top),
        left: Math.round(rect.left),
        bottom: Math.round(rect.bottom),
        right: Math.round(rect.right),
        width: Math.round(rect.width),
        height: Math.round(rect.height)
    };
};

(function () {
    let resizeHandler = null;

    window.registerResizeHandler = (dotNetRef) => {
        if (resizeHandler) {
            window.removeEventListener("resize", resizeHandler);
        }

        resizeHandler = () => {
            dotNetRef.invokeMethodAsync("UpdateTooltipPosition");
        };

        window.addEventListener("resize", resizeHandler);
    };
})();