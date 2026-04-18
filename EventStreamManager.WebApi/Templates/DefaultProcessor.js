class ProcessResult {
    constructor() {
        this.needToSend = true;        // 标志位：是否需要发送到API
        this.reason = '';              // 原因：如果不需要发送，说明原因
        this.error = null;             // 异常信息：如果try catch捕获到异常
        this.requestInfo = null;       // 请求信息：正常时的请求数据
    }

    setSuccess(requestInfo) {
        this.needToSend = true;
        this.reason = '';
        this.error = null;
        this.requestInfo = requestInfo;
        return this;
    }

    setFailure(reason, error = null) {
        this.needToSend = false;
        this.reason = reason;
        this.error = error;
        this.requestInfo = null;
        return this;
    }

    setNoSend(reason = '') {
        this.needToSend = false;
        this.reason = reason || '仅执行脚本，无需发送';
        this.error = null;
        this.requestInfo = null;
        return this;
    }
}

/**
 * 数据处理函数
 * @param {Object} data - 输入数据
 * @returns {ProcessResult} 处理结果
 */
function process(data) {
    //tip 当前使用的库函数均为对脚本引擎进行的函数注入，如需额外库函数需要实现相应函数插件
    // 在这里编写你的数据处理逻辑
    const result = new ProcessResult();
    console_log('收到数据:', data);
    return result.setNoSend('示例');
}
