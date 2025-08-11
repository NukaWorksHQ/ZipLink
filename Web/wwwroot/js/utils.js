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

window.getTooltipSize = () => {
    const tooltip = document.querySelector('.tooltip');
    if (!tooltip) {
        console.warn('Tooltip not found');
        return { height: 0, width: 0 };
    }

    const rect = tooltip.getBoundingClientRect();
    return {
        height: Math.round(rect.height),
        width: Math.round(rect.width)
    };
};

window.getViewportSize = () => {
    return {
        height: window.innerHeight,
        width: window.innerWidth
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

// Custom selector utilities
window.addOutsideClickListener = (element, dotNetRef, methodName) => {
    const handleOutsideClick = (event) => {
        if (element && !element.contains(event.target)) {
            dotNetRef.invokeMethodAsync(methodName);
        }
    };
    
    document.addEventListener('click', handleOutsideClick);
    return handleOutsideClick;
};

window.removeOutsideClickListener = (handler) => {
    if (handler) {
        document.removeEventListener('click', handler);
    }
};