(function (window) {
    async function analyzeScan(file) {
        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch('/api/scans/analyze', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        if (!response.ok) {
            throw new Error(data.error || 'Scan analysis failed');
        }

        return data;
    }

    window.NutriScanApi = {
        analyzeScan
    };
})(window);
