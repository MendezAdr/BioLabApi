using System;
using System.Collections.Generic;
using BioLabProject.Models;

namespace BioLabProject.Models;

public record OperationResult(bool Success, string? Message);

public record ObjectOperationResult(bool Success, string? Message, Object? objeto) : OperationResult(Success, Message);

public record ListOperationResult<T>(bool Success, string Message, List<T>? Data)
        : OperationResult(Success, Message);