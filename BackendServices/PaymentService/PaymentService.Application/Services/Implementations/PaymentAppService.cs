using AutoMapper;
using PaymentService.Application.DTOs;
using PaymentService.Application.Repositories;
using PaymentService.Application.Services.Abstractions;
using PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Services.Implementations
{
    public class PaymentAppService : IPaymentAppService
    {
        IPaymentRepository _paymentRepository;
        IMapper _mapper;
        public PaymentAppService(IPaymentRepository paymentRepository, IMapper mapper) {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }
       public  bool SavePaymentDetails(PaymentDetailDTO model)
        {

            PaymentDetail payment = _mapper.Map<PaymentDetail>(model);
            return _paymentRepository.SavePayementDetails(payment);
        }
            
    }
}
